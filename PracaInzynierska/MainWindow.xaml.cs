using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PracaInzynierska
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client;

        public MainWindow()
        {
            InitializeComponent();
            client = new HttpClient();
        }

        private void DownloadInstaller(string link, string name)
        {
            using (var s = client.GetStreamAsync(link))
            {
                System.IO.Directory.CreateDirectory("temp");
                using (var fs = new FileStream("temp\\" + name + ".exe", FileMode.OpenOrCreate))
                {
                    try
                    {
                        s.Result.CopyTo(fs);
                        error.Text = "Pobrano " + fs.Name;
                    }
                    catch (Exception ex)
                    {
                        error.Text = ex.Message;
                    }
                }
            }
        }

        private static bool InstallProgram(string path, string parameters)
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

        private void dwnload_Click(object sender, RoutedEventArgs e)
        {
            DownloadInstaller("https://www.rarlab.com/rar/winrar-x64-701.exe", "WinRar");
        }

        private void install_Click(object sender, RoutedEventArgs e)
        {
            if(InstallProgram("C:\\Users\\Tomek\\source\\repos\\PracaInzynierska\\PracaInzynierska\\bin\\Debug\\net8.0-windows\\temp\\WinRar.exe", "/S"))
                error.Text = "Zainstalowano!";
            else
                error.Text = "Nie udało się zainstalować programu!";
        }
    }
}