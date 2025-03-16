using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaInzynierska
{
    public class Utilities
    {
        public void AddProgramEntries(RegistryKey key, ObservableCollection<InstalledApplications> installed)
        {
            foreach (string subkey_name in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                {
                    if (subkey.GetValue("DisplayName") != null)
                    {
                        if (subkey.GetValue("DisplayVersion") != null)
                            installed.Add(new InstalledApplications() { Name = subkey.GetValue("DisplayName").ToString(), Version = subkey.GetValue("DisplayVersion").ToString() });
                        else
                            installed.Add(new InstalledApplications() { Name = subkey.GetValue("DisplayName").ToString() });
                    }
                }
            }
        }

        public void ListPrograms(ObservableCollection<InstalledApplications> installed)
        {
            installed.Clear();

            string key = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            string key2 = @"SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(key))
            {
                AddProgramEntries(k, installed);
            }
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(key2))
            {
                AddProgramEntries(k, installed);
            }
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(key))
            {
                AddProgramEntries(k, installed);
            }
        }

        public SoftwareItem ReadSoftwareItem(SqlDataReader reader, ObservableCollection<InstalledApplications> installed, bool isTemplate, string templateName)
        {
            var item = new SoftwareItem();
            item.SoftwareId = reader.GetInt32("softwareId");
            item.Name = reader.GetString("name");
            item.Version = reader.GetString("currentVersion");
            item.WebsiteLink = reader.GetString("websiteLink");
            item.DownloadLink = reader.GetString("downloadLink");
            item.Creator = reader.GetString("companyName");
            item.LastUpdate = reader.GetDateTime("updateDate").ToString("dd MMMM, yyyy");
            if (isTemplate)
                item.Category = templateName;
            else
                item.Category = reader.GetString("category");
            item.ParameterSilent = reader.GetString("parameterSilent");
            item.ParameterDirectory = reader.GetString("parameterDir");
            InstalledApplications installedApp = InstalledApplications.FindApp(item.Name, installed);
            if (installedApp != null)
                item.CurrentVersion = installedApp.Version;
            item.DownloadCount = reader.GetInt32("downloadCount");
            return item;
        }

        public int CompareVersions(string oldVersion, string newVersion)
        {
            try
            {
                if (oldVersion == String.Empty)
                    return -1;
                else
                {
                    Version oldV = new Version(oldVersion);
                    Version newV = new Version(newVersion);
                    return oldV.CompareTo(newV);
                }
            }
            catch (Exception ex)
            {
                return -2;
            }

        }
    }
}
