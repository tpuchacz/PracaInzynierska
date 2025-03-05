using PracaInzynierska;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace UnitTesting
{
    public class UnitTest1
    {
        string connStr = "server=localhost;database=inz;integrated Security=True;TrustServerCertificate=true";
        MainCode code;

        public UnitTest1()
        {
            code = new MainCode(connStr);
        }

        [Fact]
        public async Task InvalidLinkReturnsFalse()
        {
            string invalidLink = "https://sfadfsdfasdfasd.pl/download-file";
            string filePath = "temp\\somefile.exe";
            Version fileVersion = new Version("1.0.0");
            Assert.False(await code.DownloadInstaller(invalidLink, filePath, fileVersion));
        }

        [Fact]
        public async Task ValidLinkReturnsTrue()
        {
            string validLink = "https://www.win-rar.com/fileadmin/winrar-versions/winrar/winrar-x64-710.exe";
            string fileName = "WinRar";
            Version fileVersion = new Version("7.0.1");
            Assert.True(await code.DownloadInstaller(validLink, fileName, fileVersion));
        }

        [Fact]
        public async Task FindAppSuccess()
        {
            ObservableCollection<InstalledApplications> apps = new ObservableCollection<InstalledApplications>();
            InstalledApplications app = new InstalledApplications();
            app.Name = "WinRar";
            app.Version = "1.0.0";
;           apps.Add(app);

            Assert.Equal(apps[0], InstalledApplications.FindApp("WinRar", apps));
        }

        [Fact]
        public async Task FindAppFailure()
        {
            ObservableCollection<InstalledApplications> apps = new ObservableCollection<InstalledApplications>();
            InstalledApplications app = new InstalledApplications();
            app.Name = "7-Zip";
            app.Version = "1.0.0";
            apps.Add(app);
            Assert.Null(InstalledApplications.FindApp("WinRar", apps));
        }

        [Fact]
        public void IncreaseTelemetryIdExists()
        {
            Assert.True(code.IncreaseTelemetryData(1));
        }
        [Fact]
        public void IncreaseTelemetryIdDoesntExists()
        { 
            Assert.False(code.IncreaseTelemetryData(-1));
        }
    }
}