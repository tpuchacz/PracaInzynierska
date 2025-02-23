using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Microsoft.Data.SqlClient;
using Microsoft.Win32.TaskScheduler;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;

namespace PracaInzynierska
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/SetUpdateCheck")
                {
                    if(e.Args.Length < 2)
                    {
                        HandyControl.Controls.MessageBox.Show("Po wybraniu /SetUpdateCheck musi znaleźć się jedno z poniższych:\n\n/Daily - sprawdzanie aktualizacji dziennie\n/Every3Days - co 3 dni\n/Weekly - co tydzień",
                                "Niepoprawna struktura argumentów", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    else
                    {
                        if (e.Args[i + 1] == "/Daily")
                        {
                            CreateTask(1);  //Utwórz lub zmodyfikuj cykliczne sprawdzanie dostępności aktualizacji na jeden dzień
                            HandyControl.Controls.MessageBox.Show("Rozpoczęto lub zmodyfikowano sprawdzanie aktualizacji na codziennie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (e.Args[i + 1] == "/Weekly")
                        {
                            CreateTask(3);  //Utwórz lub zmodyfikuj cykliczne sprawdzanie dostępności aktualizacji na co 3 dni
                            HandyControl.Controls.MessageBox.Show("Rozpoczęto lub zmodyfikowano sprawdzanie aktualizacji na cotygodniowo!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (e.Args[i + 1] == "/Every3Days")
                        {
                            CreateTask(7);  //Utwórz lub zmodyfikuj cykliczne sprawdzanie dostępności aktualizacji na co tydzień
                            HandyControl.Controls.MessageBox.Show("Rozpoczęto lub zmodyfikowano sprawdzanie aktualizacji na co trzeci dzień", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            HandyControl.Controls.MessageBox.Show("Po wybraniu /SetUpdateCheck musi znaleźć się jedno z poniższych:\n/Daily - sprawdzanie aktualizacji dziennie\n/Every3Days - co 3 dni\n/Weekly - co tydzień",
                                "Niepoprawna struktura argumentów", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        }
                    }

                    //HandyControl.Controls.MessageBox.Show("A new version has been detected! Do you want to update?", "Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }
                else if(e.Args[i] == "/CheckForUpdates") //Sprawdź dostępność aktualizacji jednorazowo
                {
                    Utilities utils = new Utilities();
                    ObservableCollection<InstalledApplications> installed = new ObservableCollection<InstalledApplications>();
                    utils.ListPrograms(installed);

                    using (SqlConnection _con = new SqlConnection(connectionString))
                    {
                        string queryStatement = "SELECT s.softwareId, s.[name], s.[websiteLink], s.[downloadLink], s.[companyName], s.[currentVersion], s.[updateDate], s.[category]," +
                            "s.[parameterSilent], s.[parameterDir], t.downloadCount FROM ((dbo.software as s INNER JOIN dbo.telemetryData as t ON s.softwareId = t.telemetryId))";

                        try
                        {
                            bool updateAvailable = false;
                            _con.Open();
                            using (var cmd = new SqlCommand(queryStatement, _con))
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int softwareId = reader.GetInt32("softwareId");
                                    string name = reader.GetString("name");
                                    string version = reader.GetString("currentVersion");
                                    InstalledApplications installedApp = InstalledApplications.FindApp(name, installed);
                                    if (installedApp != null)
                                    {
                                        Version newVersion = new Version(version);
                                        Version oldVersion = new Version(installedApp.Version);

                                        int versionComparison = oldVersion.CompareTo(newVersion);
                                        if (versionComparison < 0)
                                        {
                                            updateAvailable = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (updateAvailable)
                            {
                                HandyControl.Controls.MessageBox.Show("Dostępna jest nowa wersja niektórych aplikacji!\n Otwórz menadżer oprogramowanai i je zainstaluj", "Dostępne aktualizacje", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                HandyControl.Controls.MessageBox.Show("Wszystkie programy zainstalowane w menadżerze są aktualne!", "Brak aktualizacji", MessageBoxButton.OK, MessageBoxImage.Information);
                            }

                        }
                        catch (Exception ex)
                        {
                            //
                        }
                        finally { _con.Close(); }
                    }
                }
            }
        }

        private void CreateTask(int repetition)
        {
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task task = ts.FindTask("SoftwareManagerUpdateChecker");
                if (task != null)
                {
                    ts.RootFolder.DeleteTask("SoftwareManagerUpdateChecker");
                }
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Zdarzenie sprawdzające dostępność aktualizacji programów z listy menadżera oprogramowania";
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Settings.DeleteExpiredTaskAfter = TimeSpan.Zero; //Zadanie nie zostanie usunięte

                var dailyTrigger = new DailyTrigger();
                dailyTrigger.Repetition.Interval = TimeSpan.FromDays(repetition);  //Powtarza co jeden dzień zadanie codziennie
                dailyTrigger.Repetition.Duration = TimeSpan.FromDays(30); //Przez okres 30 dni


                td.Triggers.Add(dailyTrigger);
                td.Actions.Add(new ExecAction(Directory.GetCurrentDirectory() + "\\PracaInzynierska.exe", "/CheckForUpdates", ""));

                ts.RootFolder.RegisterTaskDefinition(@"SoftwareManagerUpdateChecker", td);
            }
        }
    }

}
