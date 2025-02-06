using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PracaInzynierska
{
    public partial class MainWindow : Window
    {
        private MainCode code;
        public MainWindow()
        {
            InitializeComponent();
            code = new MainCode();
            listViewTest.ItemsSource = code.SoftwareItems;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listViewTest.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            view.GroupDescriptions.Add(groupDescription);
        }
    }
}