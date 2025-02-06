using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private DateTime? lastUpdate;

        [ObservableProperty]
        private string? category;

        [ObservableProperty]
        private string? parameterSilent;

        [ObservableProperty]
        private string? parameterDirectory;

    }
}
