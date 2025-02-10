using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PracaInzynierska
{
    public partial class MainCode : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<InstalledApplications> appList;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> selectedSoftwareItems;

        HttpClient client;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> softwareItems;
        public MainCode()
        {
            SoftwareItems = new ObservableCollection<SoftwareItem>();
            FetchSoftwareItems();
            client = new HttpClient();
            SelectedSoftwareItems = new ObservableCollection<SoftwareItem>();
            progressText = "Czekam na operacje...";
        }

        private void FetchSoftwareItems()
        {
            ListPrograms();
            SoftwareItems.Clear();
            string connectionString = "server=localhost;database=inz;integrated Security=True;TrustServerCertificate=true"; //W pliku konfiguracyjnym

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "SELECT * FROM dbo.software";

                _con.Open();
                using (var cmd = new SqlCommand(queryStatement, _con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new SoftwareItem();
                        item.Name = reader.GetString("name");
                        item.Version = reader.GetString("currentVersion");
                        item.WebsiteLink = reader.GetString("websiteLink");
                        item.DownloadLink = reader.GetString("downloadLink");
                        item.Creator = reader.GetString("companyName");
                        item.Version = reader.GetString("currentVersion");
                        item.LastUpdate = reader.GetDateTime("updateDate").ToString("dd MMMM, yyyy"); 
                        item.Category = reader.GetString("category");
                        item.ParameterSilent = reader.GetString("parameterSilent");
                        item.ParameterDirectory = reader.GetString("parameterDir");
                        InstalledApplications installedApp = InstalledApplications.FindApp(item.Name, AppList);
                        if (installedApp != null)
                            item.CurrentVersion = installedApp.Version;
                        SoftwareItems.Add(item);
                    }
                }
            }
            OnPropertyChanged(nameof(SoftwareItems));
        }

        [RelayCommand]
        public void StartInstallationProcess()
        {
            bool installed = false;
            ProgressText = string.Empty;
            for(int i = 0; i < SelectedSoftwareItems.Count; i++)
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
                    DownloadInstaller(SelectedSoftwareItems[i].DownloadLink, SelectedSoftwareItems[i].Name);
                    var result = InstallPrograms("temp\\" + SelectedSoftwareItems[i].Name + ".exe", SelectedSoftwareItems[i].ParameterSilent);
                    if (result)
                    {
                        ProgressText += $"Poprawnie zainstalowano program {SelectedSoftwareItems[i].Name} w wersji {SelectedSoftwareItems[i].Version}\n";
                        installed = true;
                    }
                    else
                    {
                        ProgressText += $"Błąd podczas instalacji {SelectedSoftwareItems[i].Name}\n";
                    }
                }
                else if (versionComparison > 0)
                {
                    ProgressText += $"Program {SelectedSoftwareItems[i].Name} posiada wersję nowszą niż tą w bazie danych!\n";
                }
                else
                {
                    ProgressText += $"Program {SelectedSoftwareItems[i].Name} posiada już najnowszą wersję!\n";
                }
            }
            if(installed)
            {
                ListPrograms();
                FetchSoftwareItems();
            }
        }

        public bool DownloadInstaller(string link, string name)
        {
            using (var s = client.GetStreamAsync(link))
            {
                Directory.CreateDirectory("temp");
                using (var fs = new FileStream("temp\\" + name + ".exe", FileMode.OpenOrCreate))
                {
                    try
                    {
                        s.Result.CopyTo(fs);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ProgressText = ex.Message;
                        return false;
                    }
                }
            }
        }

        private bool InstallPrograms(string path, string parameters)
        {
            try
            {
                ProcessStartInfo processStartInfo;
                Process process;

                processStartInfo = new ProcessStartInfo();
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.Arguments = parameters;
                processStartInfo.FileName = path;

                process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                while (!process.HasExited && process.Responding)
                {
                    Thread.Sleep(100);
                }

                return true;
            }
            catch (Exception ex)
            {
                ProgressText += $"\n{ex.Message}";
                return false;
            }
        }

        private void ListPrograms()
        {
            AppList = new ObservableCollection<InstalledApplications>();
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        if(subkey.GetValue("DisplayName") != null)
                        {
                            if(subkey.GetValue("DisplayVersion") != null)
                            {
                                AppList.Add(new InstalledApplications() { Name = subkey.GetValue("DisplayName").ToString(), Version = subkey.GetValue("DisplayVersion").ToString() });
                            }
                            else
                            {
                                AppList.Add(new InstalledApplications() { Name = subkey.GetValue("DisplayName").ToString() });
                            }
                        }
                            
                    }
                }
            }
        }
    }
}
