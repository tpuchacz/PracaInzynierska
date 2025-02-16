using CommunityToolkit.Mvvm.ComponentModel;

namespace PracaInzynierska
{
    public partial class SoftwareItem : ObservableObject
    {
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

    }
}
