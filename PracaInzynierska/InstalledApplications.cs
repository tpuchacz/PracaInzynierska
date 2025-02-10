using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaInzynierska
{
    public partial class InstalledApplications : ObservableObject
    {
        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? version;

        public static InstalledApplications FindApp(string name, ObservableCollection<InstalledApplications> apps)
        {
            return apps.FirstOrDefault(a => a.Name.ToLower().Contains(name.ToLower()));
        }
    }
}
