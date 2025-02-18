﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows.Input;

namespace PracaInzynierska
{
    public partial class MainCode : ObservableObject
    {
        string connectionString = "server=localhost;database=inz;integrated Security=True;TrustServerCertificate=true"; //W pliku konfiguracyjnym

        [ObservableProperty]
        private string installButtonText = "Wybierz programy...";

        [ObservableProperty]
        private bool canClickInstallButton = false;

        [ObservableProperty]
        private bool enableControls = true;

        [ObservableProperty]
        private bool operationInProgress = false;

        [ObservableProperty]
        private bool templatesShown = false;

        [ObservableProperty]
        private bool programsShown = true;

        [ObservableProperty]
        private ObservableCollection<InstalledApplications> appList;

        [ObservableProperty]
        private string progressText = "Czekam na operacje...";

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> selectedSoftwareItems; 

        HttpClient client;

        private bool afterInstalling = false;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> currentSoftwareItems;

        //[ObservableProperty]
        private ObservableCollection<SoftwareItem> softwareItems;

        //[ObservableProperty]
        private ObservableCollection<SoftwareItem> templateItems;
        public MainCode()
        {
            AppList = new ObservableCollection<InstalledApplications>();
            softwareItems = new ObservableCollection<SoftwareItem>();
            templateItems = new ObservableCollection<SoftwareItem>();
            CurrentSoftwareItems = new ObservableCollection<SoftwareItem>();
            FetchSoftwareItems();
            client = new HttpClient();
            SelectedSoftwareItems = new ObservableCollection<SoftwareItem>();
        }

        [RelayCommand]
        private void FetchSoftwareItems()
        {
            ListPrograms();
            softwareItems.Clear();

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "SELECT s.softwareId, s.[name], s.[websiteLink], s.[downloadLink], s.[companyName], s.[currentVersion], s.[updateDate], s.[category]," +
                    "s.[parameterSilent], s.[parameterDir], t.downloadCount FROM ((dbo.software as s INNER JOIN dbo.telemetryData as t ON s.softwareId = t.telemetryId))";

                try
                {
                    _con.Open();
                    using (var cmd = new SqlCommand(queryStatement, _con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new SoftwareItem();
                            softwareItems.Add(ReadSoftwareItem(reader, false, ""));
                        }
                    }

                }
                catch (Exception ex)
                { 
                    ProgressText = "Wystąpił błąd:\n" + ex.Message;
                }
                finally { _con.Close(); }
            }
            LoadTemplates();
            CurrentSoftwareItems = softwareItems.Clone();
            OnPropertyChanged(nameof(CurrentSoftwareItems));
        }

        [RelayCommand]
        private void ShowTemplates()
        {
            if(!TemplatesShown)
            {
                CurrentSoftwareItems = templateItems.Clone();
                OnPropertyChanged(nameof(CurrentSoftwareItems));
                TemplatesShown = true;
                ProgramsShown = false;
            }
        }

        [RelayCommand]
        private void ShowProgramList()
        {
            if (!ProgramsShown)
            {
                ProgramsShown = true;
                TemplatesShown = false;
                CurrentSoftwareItems = softwareItems.Clone();
            }
        }

        private SoftwareItem ReadSoftwareItem(SqlDataReader reader, bool isTemplate, string templateName)
        {
            var item = new SoftwareItem();
            item.SoftwareId = reader.GetInt32("softwareId");
            item.Name = reader.GetString("name");
            item.Version = reader.GetString("currentVersion");
            item.WebsiteLink = reader.GetString("websiteLink");
            item.DownloadLink = reader.GetString("downloadLink");
            item.Creator = reader.GetString("companyName");
            item.Version = reader.GetString("currentVersion");
            item.LastUpdate = reader.GetDateTime("updateDate").ToString("dd MMMM, yyyy");
            if(isTemplate)
                item.Category = templateName;
            else
                item.Category = reader.GetString("category");
            item.ParameterSilent = reader.GetString("parameterSilent");
            item.ParameterDirectory = reader.GetString("parameterDir");
            InstalledApplications installedApp = InstalledApplications.FindApp(item.Name, AppList);
            if (installedApp != null)
                item.CurrentVersion = installedApp.Version;
            item.DownloadCount = reader.GetInt32("downloadCount");
            return item;
        }

        private void LoadTemplates()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "SELECT * FROM dbo.templates";

                try
                {
                    _con.Open();
                    List<string> templateNames = new List<string>();
                    List<string> procedureNames = new List<string>();
                    using (var cmd = new SqlCommand(queryStatement, _con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            templateNames.Add(reader.GetString("templateName"));
                            procedureNames.Add(reader.GetString("procedureName"));
                        }
                    }

                    for (int i = 0; i < procedureNames.Count; i++)
                    {
                        using (var cmd = new SqlCommand(procedureNames[i], _con))
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                templateItems.Add(ReadSoftwareItem(reader, true, templateNames[i]));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProgressText = "Wystąpił błąd:\n" + ex.Message;
                }
                finally { _con.Close(); }
            }
        }

        private void IncreaseTelemetryData(int id)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand($"UPDATE dbo.telemetryData SET downloadCount=downloadCount + 1 WHERE telemetryId={id}", _con);
                try
                {
                    _con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ProgressText = ex.Message;
                }
                finally { _con.Close(); }
            }
        }

        

        [RelayCommand]
        public async Task StartInstallationProcess()
        {
            EnableControls = false;
            bool installed = false;
            ProgressText = string.Empty;
            string notInstalled = "";
            for (int i = 0; i < SelectedSoftwareItems.Count; i++)
            {
                Version newVersion = new Version(SelectedSoftwareItems[i].Version);
                Version oldVersion;
                if (SelectedSoftwareItems[i].CurrentVersion == null)
                {
                    oldVersion = new Version("0.0");
                }
                else
                {
                    oldVersion = new Version(SelectedSoftwareItems[i].CurrentVersion);
                }


                int versionComparison = oldVersion.CompareTo(newVersion);

                if (versionComparison < 0)
                {
                    ProgressText += "------------------------Proces instalacji " + SelectedSoftwareItems[i].Name + "------------------------\n";

                    OperationInProgress = true;
                    
                    Task<bool> downloading = DownloadInstaller(SelectedSoftwareItems[i].DownloadLink, SelectedSoftwareItems[i].Name);

                    bool downloadResult = await downloading;

                    if (!downloadResult)
                    {
                        ProgressText += $"Błąd podczas pobierania {SelectedSoftwareItems[i].Name}\n";
                        continue;
                    }

                    ProgressText += $"\tPróbuję zainstalować {SelectedSoftwareItems[i].Name}...\n";

                    Task<bool> installing;

                    if (SelectedSoftwareItems[i].ParameterDirectory != String.Empty)
                    {
                        installing = InstallPrograms("temp\\" + SelectedSoftwareItems[i].Name + ".exe", SelectedSoftwareItems[i].ParameterSilent +
                            " " + SelectedSoftwareItems[i].ParameterDirectory + "C:\\ManagerApps\\" + SelectedSoftwareItems[i].Name);
                    }
                    else
                    {
                        installing = InstallPrograms("temp\\" + SelectedSoftwareItems[i].Name + ".exe", SelectedSoftwareItems[i].ParameterSilent);
                    }

                    bool installResult = await installing;

                    await Task.Delay(1000); //Czekanie na wpisanie zmian do rejestru itd. Program będzie bez tego zainstalowany, ale lista się nie odświeży.


                    OperationInProgress = false;
                    afterInstalling = true;
                    if (installResult)
                    {
                        ProgressText += $" + Poprawnie zainstalowano program {SelectedSoftwareItems[i].Name} w wersji {SelectedSoftwareItems[i].Version}\n";
                        installed = true;
                        IncreaseTelemetryData(SelectedSoftwareItems[i].SoftwareId);
                    }
                    else
                    {
                        notInstalled += "\t" + SelectedSoftwareItems[i].Name + "\n";
                        ProgressText += $"\tBłąd podczas instalacji {SelectedSoftwareItems[i].Name}\n";
                    }
                    ProgressText += "------------------------Koniec instalacji " + SelectedSoftwareItems[i].Name + "------------------------\n";
                }
                else if (versionComparison > 0)
                {
                    ProgressText += $"-Program {SelectedSoftwareItems[i].Name} posiada wersję nowszą niż tą w bazie danych!\n";
                }
                else
                {
                    ProgressText += $"-Program {SelectedSoftwareItems[i].Name} posiada już najnowszą wersję!\n";
                }
            }
            if (installed) //Do poprawy
            {
                FetchSoftwareItems();
            }
            else
            {
                if(notInstalled != String.Empty)
                {
                    ProgressText += " ! Nie zainstalowano poniższych programów z powodu błędów: ! \n";
                    ProgressText += notInstalled;
                }
            }
            EnableControls = true;
        }

        public async Task<bool> DownloadInstaller(string link, string name)
        {
            if (!File.Exists("temp\\" + name + ".exe"))
            {
                ProgressText += $"Pobieram {name}...\n";
                using (var s = await client.GetStreamAsync(link)) //Czekanie na sprawdzenie dostępności pliku
                {
                    Directory.CreateDirectory("temp");
                    using (var fs = new FileStream("temp\\" + name + ".exe", FileMode.OpenOrCreate))
                    {
                        try
                        {
                            await s.CopyToAsync(fs); //Czekanie na zapisanie pliku na dysku
                            return true;
                        }
                        catch (Exception ex)
                        {
                            ProgressText += ex.Message + "\n";
                            return false;
                        }
                    }
                }
            }
            else
            {
                ProgressText += $"\tPlik instalacyjny dla {name} już istnieje...\n";
                return true;
            }
        }

        private async Task<bool> InstallPrograms(string path, string parameters)
        {
            try
            {
                ProcessStartInfo processStartInfo;
                Process process;

                processStartInfo = new ProcessStartInfo();
                processStartInfo.CreateNoWindow = true;
                processStartInfo.Arguments = parameters;
                processStartInfo.FileName = path;

                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                await process.WaitForExitAsync();
                string appName = new DirectoryInfo(path).Name;
                if (InstalledApplications.FindApp(appName, AppList) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ProgressText += $"\n{ex.Message}";
                return false;
            }
        }

        private void ListPrograms()
        {
            AppList.Clear();
            
            string key =  @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            string key2 = @"SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(key))
            {
                AddProgramEntries(k);
            }
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(key2))
            {
                AddProgramEntries(k);
            }
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(key))
            {
                AddProgramEntries(k);
            }
        }

        private void AddProgramEntries(RegistryKey key)
        {
            foreach (string subkey_name in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                {
                    if (subkey.GetValue("DisplayName") != null)
                    {
                        if (subkey.GetValue("DisplayVersion") != null)
                            AppList.Add(new InstalledApplications() { Name = subkey.GetValue("DisplayName").ToString(), Version = subkey.GetValue("DisplayVersion").ToString() });
                        else
                            AppList.Add(new InstalledApplications() { Name = subkey.GetValue("DisplayName").ToString() });
                    }
                }
            }
        }

        [RelayCommand]
        private void SelectionChanged(object parameter)
        {
            SelectedSoftwareItems.Clear();

            if(parameter != null)
            {
                IList<object> selItem = parameter as IList<object>;

                foreach (var temp in selItem)
                {
                    SoftwareItem si = temp as SoftwareItem;
                    SelectedSoftwareItems.Add(si);
                }
            }


            if (afterInstalling)
            {
                afterInstalling = false;
            }
            else
            {
                ProgressText = "Czekam na operacje...";
            }
            

            if (SelectedSoftwareItems.Count == 0)
            {
                CanClickInstallButton = false;
                
            }
            else
            {
                if(SelectedSoftwareItems.Count == 1)
                    InstallButtonText = "Zainstaluj 1 program...";
                else
                    InstallButtonText = "Zainstaluj " + SelectedSoftwareItems.Count + " programy...";
                CanClickInstallButton = true;
                ProgressText = "Do zainstalowania:\n";
                foreach (SoftwareItem item in SelectedSoftwareItems)
                {
                    ProgressText += " + " + item.Name + "\n";
                }
            }
        }

        [RelayCommand]
        private void Search(string parameter)
        {
            if(parameter != String.Empty)
            {
                parameter = parameter.ToLower();
                foreach (SoftwareItem item in CurrentSoftwareItems)
                {
                    if (item.Name.ToLower().Contains(parameter))
                    {
                        item.IsHidden = false;
                    }
                    else if(item.Creator.ToLower().Contains(parameter))
                    {
                        item.IsHidden = false;
                    }
                    else
                    {
                        item.IsHidden = true;
                    }
                }
            }
            else
            {
                foreach (SoftwareItem item in CurrentSoftwareItems)
                {
                    item.IsHidden = false;
                }
            }
        }
    }
}