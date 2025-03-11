using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PracaInzynierska
{
    public partial class SoftwareItem : ObservableObject
    {
        [ObservableProperty]
        private int softwareId;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? version;

        [ObservableProperty]
        private string? websiteLink;

        [ObservableProperty]
        private string? downloadLink;

        [ObservableProperty]
        private string? creator;

        [ObservableProperty]
        private string? lastUpdate;

        [ObservableProperty]
        private string? category;

        [ObservableProperty]
        private string? parameterSilent;

        [ObservableProperty]
        private string? parameterDirectory;

        [ObservableProperty]
        private string? currentVersion;

        [ObservableProperty]
        private bool isHidden = false;

        [ObservableProperty]
        private int downloadCount;

        public static bool IdExistsInCollection(ObservableCollection<SoftwareItem> items, int id)
        {
            foreach (SoftwareItem item in items)
            {
                if (item.SoftwareId == id)
                    return true;
            }
            return false;
        }
    }
}
