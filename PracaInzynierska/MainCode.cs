using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PracaInzynierska
{
    public partial class MainCode : ObservableObject
    {
        [ObservableProperty]
        private SoftwareItem selectedSoftwareItem;

        HttpClient client;

        [ObservableProperty]
        private ObservableCollection<SoftwareItem> softwareItems;

        public MainCode()
        {
            client = new HttpClient();
            FetchSoftwareItems();
        }

        private void FetchSoftwareItems()
        {
            softwareItems = new ObservableCollection<SoftwareItem>();

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
                        item.LastUpdate = reader.GetDateTime("updateDate");
                        item.Category = reader.GetString("category");
                        item.ParameterSilent = reader.GetString("parameterSilent");
                        item.ParameterDirectory = reader.GetString("parameterDir");
                        softwareItems.Add(item);
                    }
                }
            }
        }

        public bool DownloadInstaller(string link, string name)
        {
            using (var s = client.GetStreamAsync(link))
            {
                System.IO.Directory.CreateDirectory("temp");
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
        private static bool InstallPrograms(string path, string parameters) //InstallPrograms, możliwie kilka naraz
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = parameters;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
