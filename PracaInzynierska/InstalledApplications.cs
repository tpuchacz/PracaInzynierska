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
            for (int i = 0; i < apps.Count; i++)
            {
                if (apps[i].Name.ToLower().Contains(name.ToLower()))
                    return apps[i];
            }
            return null;
        }
    }
}
