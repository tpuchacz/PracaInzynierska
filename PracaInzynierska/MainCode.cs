using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
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
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PracaInzynierska
{
    public partial class MainCode : ObservableObject
    {
        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> selectedSoftwareItems;

        HttpClient client;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> softwareItems;
        public MainCode()
        {
            FetchSoftwareItems();
            client = new HttpClient();
            SelectedSoftwareItems = new ObservableCollection<SoftwareItem>();
            progressText = "Czekam na operacje...";
        }

        private void FetchSoftwareItems()
        {
            string connectionString = "server=localhost;database=inz;integrated Security=True;TrustServerCertificate=true"; //W pliku konfiguracyjnym

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "SELECT * FROM dbo.software";

                _con.Open();
                SoftwareItems = new ObservableCollection<SoftwareItem>();
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
                        SoftwareItems.Add(item);
                    }
                }
            }
        }

        [RelayCommand]
        public void StartInstallationProcess()
        {
            for(int i = 0; i < SelectedSoftwareItems.Count; i++)
            {
                DownloadInstaller(SelectedSoftwareItems[i].DownloadLink, SelectedSoftwareItems[i].Name);
                var result = InstallPrograms("temp\\" + SelectedSoftwareItems[i].Name + ".exe", SelectedSoftwareItems[i].ParameterSilent);
                ProgressText += result.Result.ToString();
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
                        return false;
                    }
                }
            }
        }

        //Ścieżka ustalana w opcjach programu (lub na sztywno), parametry z bazy danych
        private async Task<bool> InstallPrograms(string path, string parameters) //InstallPrograms, możliwie kilka naraz
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = parameters;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"Output: {e.Data}");
                        ProgressText += $"\n{e.Data}";
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
