using System.Windows;
using System.Windows.Controls;

namespace PracaInzynierska
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainCode code = new MainCode("", null, null);
            DataContext = code;    
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        => listViewPrograms.SelectedItems.Clear();

    }
}