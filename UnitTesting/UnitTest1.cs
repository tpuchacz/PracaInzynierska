using Azure.Core;
using Microsoft.Data.SqlClient;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using PracaInzynierska;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media.Converters;

namespace UnitTesting
{
    /*Klasa wykorzystana do testów jednostkowych.
     * stworzona jest baza danych zawieraj¹ca kilka przyk³adowych wpisów i dzia³a ona niezale¿nie od g³ównej bazy danych.
     * Testy integracyjne w osobnej klasie s¹ przeprowadzane na g³ównej bazie danych.
     */
    public class UnitTest1
    {
        string connStr = "Server=sampleServer;Database=sampleDB;Trusted_Connection=True;";
        MainCode code;

        public UnitTest1()
        {
            code = new MainCode(connStr, null, null);
        }

        [Fact]
        public async Task InvalidLinkReturnsFalse()
        {
            var invalidLink = "https://invalid-link.pl/download-file";
            var fileName = "somefile.exe";
            var version = "1.0.0";

            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()) 
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Not Found")
                });

            var mockHttpClient = new HttpClient(mockHandler.Object);
            var code = new MainCode(connStr, null, mockHttpClient); 
            var result = await code.DownloadInstaller(invalidLink, fileName, version);

            Assert.False(result); 
        }


        [Fact]
        public async Task ValidLinkReturnsTrue()
        {
            string validLink = "https://valid-link.pl/download-file";
            var fileName = "somefile.exe";
            var version = "1.0.0";

            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Valid file content")))
                });

            var mockHttpClient = new HttpClient(mockHandler.Object);
            var code = new MainCode(connStr, null, mockHttpClient);
            var result = await code.DownloadInstaller(validLink, fileName, version);

            Assert.True(result);
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
            var mockDbHelper = new Mock<IDatabaseService>();
            mockDbHelper.Setup(db => db.ExecuteNonQuery(It.IsAny<string>())).Returns(1);
            code = new MainCode(connStr, mockDbHelper.Object, null);

            int result = code.IncreaseTelemetryData(1);
            Assert.Equal(1, result);
        }
        [Fact]
        public void IncreaseTelemetryIdDoesntExists()
        {
            var mockDbHelper = new Mock<IDatabaseService>();
            mockDbHelper.Setup(db => db.ExecuteNonQuery(It.IsAny<string>())).Returns(0);
            code = new MainCode(connStr, mockDbHelper.Object, null);

            int result = code.IncreaseTelemetryData(-1);
            Assert.Equal(0, result);
        }

        [Fact]
        public void CompareVersionsOldIsLower()
        {
            string oldVersion = "1.0.0";
            string newVersion = "2.0.1";
            Assert.Equal(-1, code.CompareVersions(oldVersion, newVersion));
        }
        [Fact]
        public void CompareVersionsOldEqualToNew()
        {
            string oldVersion = "1.0.0";
            string newVersion = "1.0.0";
            Assert.Equal(0, code.CompareVersions(oldVersion, newVersion));
        }
        [Fact]
        public void CompareVersionsOldIsHigher()
        {
            string oldVersion = "2.0.1";
            string newVersion = "1.0.0";
            Assert.Equal(1, code.CompareVersions(oldVersion, newVersion));
        }
        [Fact]
        public void CompareVersionsNotValid()
        {
            string oldVersion = "abcdef";
            string newVersion = "ghijkl";
            Assert.Equal(-2, code.CompareVersions(oldVersion, newVersion));
        }
        [Fact]
        public void CompareVersionsNoCurrentVersion()
        {
            string oldVersion = "";
            string newVersion = "2.0.0";
            Assert.Equal(-1, code.CompareVersions(oldVersion, newVersion));
        }
    }
}