using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace PracaInzynierska
{
    public partial class MainCode : ObservableObject
    {
        string connectionString; //W pliku konfiguracyjnym

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
        private string progressText = "Czekam na operacje...";

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> selectedSoftwareItems; 

        HttpClient client;

        private bool afterInstalling = false;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> currentSoftwareItems;

        private ObservableCollection<SoftwareItem> softwareItems;

        private ObservableCollection<SoftwareItem> templateItems;

        private ObservableCollection<InstalledApplications> appList;

        private readonly IDatabaseService databaseService;

        Utilities utils;
        public MainCode(string connStr, IDatabaseService dbService, HttpClient client)
        {
            if(connStr == "")
            {
                try
                {
                    connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();
                }
                catch (ConfigurationErrorsException ex)
                {
                    connectionString = "";
                    ProgressText = "Błąd przy wczytywaniu konfiguracji: " + ex.Message + "\n";
                }
            }
            else
            {
                connectionString = connStr;
            }

            

            if(dbService == null)
                databaseService = new DatabaseService(connectionString);
            else
                databaseService = dbService;
            
            utils = new Utilities();

            if (client == null)
            {
                this.client = new HttpClient();
                string customUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36";
                this.client.DefaultRequestHeaders.Add("User-Agent", customUserAgent);
            }
            else
                this.client = client;

            

            appList = new ObservableCollection<InstalledApplications>();
            softwareItems = new ObservableCollection<SoftwareItem>();
            templateItems = new ObservableCollection<SoftwareItem>();
            CurrentSoftwareItems = new ObservableCollection<SoftwareItem>();
            softwareItems = FetchSoftwareItems(appList);
            templateItems = LoadTemplates(appList);
            CurrentSoftwareItems = softwareItems;
            SelectedSoftwareItems = new ObservableCollection<SoftwareItem>();
        }

        [RelayCommand]
        private void CallFetchSoftwareItems()
        {
            softwareItems = FetchSoftwareItems(appList);
            templateItems = LoadTemplates(appList);
            if (ProgramsShown)
                CurrentSoftwareItems = softwareItems;
            else
                CurrentSoftwareItems = templateItems.Clone();
        }

        partial void OnProgramsShownChanged(bool value)
        {
            if (value)
                CurrentSoftwareItems = softwareItems;
            else
                CurrentSoftwareItems = templateItems;
        }

        private ObservableCollection<SoftwareItem> FetchSoftwareItems(ObservableCollection<InstalledApplications> applist)
        {
            utils.ListPrograms(applist);
            ObservableCollection<SoftwareItem> si = new ObservableCollection<SoftwareItem>();

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "SELECT s.softwareId, s.[name], s.[websiteLink], s.[downloadLink], s.[companyName], s.[currentVersion], s.[updateDate], s.[category]," +
                    "s.[parameterSilent], s.[parameterDir], t.downloadCount FROM ((dbo.software as s INNER JOIN dbo.telemetryData as t ON s.softwareId = t.telemetryId))";

                try
                {
                    _con.Open();
                    using (var cmd = new SqlCommand(queryStatement, _con))
                    using (SqlDataReader reader = databaseService.ExecuteReader(queryStatement))
                    {
                        while (reader.Read())
                        {
                            si.Add(utils.ReadSoftwareItem(reader, applist, false, ""));
                        }
                    }

                }
                catch (Exception ex)
                { 
                    ProgressText = "Wystąpił błąd:\n" + ex.Message + "\n";
                }
                finally { _con.Close(); }
            }
            return si;
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

        private ObservableCollection<SoftwareItem> LoadTemplates(ObservableCollection<InstalledApplications> appList)
        {
            utils.ListPrograms(appList);
            ObservableCollection<SoftwareItem> si = new ObservableCollection<SoftwareItem>();
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
                                si.Add(utils.ReadSoftwareItem(reader, appList, true, templateNames[i]));
                            }
                        }
                    }
                    return si;
                }
                catch (Exception ex)
                {
                    ProgressText = "Wystąpił błąd:\n" + ex.Message + "\n";
                    si = new ObservableCollection<SoftwareItem>();
                    return si;
                }
                finally { _con.Close(); }
            }
        }

        public int IncreaseTelemetryData(int id)
        {
            string query = $"UPDATE dbo.telemetryData SET downloadCount=downloadCount + 1 WHERE telemetryId={id}";
            return databaseService.ExecuteNonQuery(query);
        }

        [RelayCommand]
        private async Task StartInstallationProcess()
        {
            EnableControls = false;
            bool installed = false;
            ProgressText = string.Empty;
            string notInstalled = "";
            for (int i = 0; i < SelectedSoftwareItems.Count; i++)
            {
                int versionComparison = utils.CompareVersions(SelectedSoftwareItems[i].CurrentVersion, SelectedSoftwareItems[i].Version);

                if (versionComparison < 0)
                {
                    ProgressText += "------------------------Proces instalacji " + SelectedSoftwareItems[i].Name + "------------------------\n";

                    OperationInProgress = true;
                    
                    Task<bool> downloading = DownloadInstaller(SelectedSoftwareItems[i].DownloadLink, SelectedSoftwareItems[i].Name, SelectedSoftwareItems[i].Version);

                    bool downloadResult = await downloading;

                    if (!downloadResult)
                    {
                        ProgressText += $"Błąd podczas pobierania {SelectedSoftwareItems[i].Name}\n";
                        OperationInProgress = false;
                        ProgressText += "------------------------Koniec instalacji " + SelectedSoftwareItems[i].Name + "------------------------\n";
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
            if (installed)
            {
                FetchSoftwareItems(appList);
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

        public async Task<bool> DownloadInstaller(string link, string name, string fileVersion)
        {
            string filePath = "temp\\" + name + ".exe";
            if (!File.Exists(filePath))
            {
                ProgressText += $"\tPobieram {name}...\n";
                var result = await DownloadFile(link, filePath);
                return result;
            }
            else
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, filePath));
                if(fvi.FileVersion == null || String.IsNullOrWhiteSpace(fvi.FileVersion))
                {
                    ProgressText += $"\tPobieram {name}...\n";
                    var result = await DownloadFile(link, filePath);
                    return result;
                }
                else
                {
                    int versionComparison = utils.CompareVersions(fvi.FileVersion.Trim(), fileVersion);
                    if (versionComparison < 0)
                    {
                        ProgressText += $"\tPobieram {name}...\n";
                        var result = await DownloadFile(link, filePath);
                        return result;
                    }
                    else
                    {
                        ProgressText += $"\tPlik instalacyjny dla {name} już istnieje...\n";
                        return true;
                    }
                }
            }
        }

        public async Task<bool> DownloadFile(string link, string filePath)
        {
            try
            {
                Directory.CreateDirectory("temp");
                using (var s = await client.GetStreamAsync(link)) //Czekanie na sprawdzenie dostępności pliku
                {
                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        await s.CopyToAsync(fs); //Czekanie na zapisanie pliku na dysku
                        return true;
                    }
                }   
            }
            catch(Exception ex)
            {
                ProgressText += ex.Message + "\n";
                return false;
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

                process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                    

                await process.WaitForExitAsync();

                string appName = Path.GetFileNameWithoutExtension(path);
                utils.ListPrograms(appList);

                if (InstalledApplications.FindApp(appName, appList) != null)
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
                ProgressText += $"\t{ex.Message}\n";
                return false;
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
                    if(!SoftwareItem.IdExistsInCollection(SelectedSoftwareItems, si.SoftwareId))
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