using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PracaInzynierska
{
    public partial class MainWindow : Window
    {
        //private MainCode _code;
        public bool ButtonState { get; set; } = false;
        public MainWindow()
        {
            InitializeComponent();
            MainCode code = new MainCode();
            DataContext = code;
            listViewPrograms.SelectionChanged += ListViewPrograms_SelectionChanged;
        }

        private void ListViewPrograms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mc = (MainCode)DataContext;
            mc.SelectedSoftwareItems.Clear();
            for (int i = 0; i < listViewPrograms.SelectedItems.Count; i++)
            {
                mc.SelectedSoftwareItems.Add((SoftwareItem)listViewPrograms.SelectedItems[i]);
            }
            this.installButton.IsEnabled = mc.SelectedSoftwareItems.Count > 0;
            if (mc.SelectedSoftwareItems.Count == 1)
                this.installButton.Content = "Instaluj 1 program...";
            else if (mc.SelectedSoftwareItems.Count > 1)
                this.installButton.Content = "Instaluj " + mc.SelectedSoftwareItems.Count.ToString() + " programy...";
            else
                this.installButton.Content = "Wybierz programy...";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listViewPrograms.SelectedItems.Clear();
        }
    }
}