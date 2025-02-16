using System.Windows;

namespace PracaInzynierska
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainCode code = new MainCode();
            DataContext = code;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        => listViewPrograms.SelectedItems.Clear();

    }
}