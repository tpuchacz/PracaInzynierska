using Microsoft.Data.SqlClient;
using Moq;
using PracaInzynierska;
using System.Windows;
using Xunit;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using HandlebarsDotNet.Helpers.Enums;
using Humanizer;
using System.Windows.Controls.Primitives;

namespace IntegrationTesting
{
    public class IntegrationTest1
    {
        private readonly string connStr = "server=localhost;database=testing;integrated Security=True;TrustServerCertificate=true;";
        private readonly WireMockServer server;
        private readonly string testDir = "temp";
        private readonly string testFilePath;
        private readonly string testFileName = "testInstaller.exe";
        private readonly HttpClient client;
        private readonly MainCode mainCode;
        private readonly Utilities utils;
        public IntegrationTest1()
        {
            testFilePath = Path.Combine(testDir, testFileName);
            server = WireMockServer.Start();
            client = new HttpClient { BaseAddress = new Uri(server.Urls[0]) };

            server.Given(
                Request.Create().WithPath("/testInstaller.exe").UsingGet()
            ).RespondWith(
                Response.Create().WithBody("Fake installer content").WithStatusCode(200)
            );

            mainCode = new MainCode(connStr, null, client);
            utils = new Utilities();
        }

        [Fact]
        public async Task DownloadInstallerNotExisting()
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);

            bool result = await mainCode.DownloadInstaller($"{server.Urls[0]}/testInstaller.exe", "testInstaller", "1.0.0");

            Assert.True(result);
            Assert.True(File.Exists(testFilePath));
        }

        [Fact]
        public async Task DownloadInstallerExistingVersionOlder()
        {
            Directory.CreateDirectory(testDir);
            File.WriteAllText(testFilePath, "Old installer content");
            string existingVersion = "1.0.0";
            int versionComparison = utils.CompareVersions(existingVersion, "2.0.0");
            bool shouldDownload = versionComparison < 0;

            bool result = false;
            if (shouldDownload)
                result = await mainCode.DownloadInstaller($"{server.Urls[0]}/testInstaller.exe", "testInstaller", "2.0.0");
            else
                Assert.Fail();
            Assert.True(result);
            Assert.True(File.Exists(testFilePath));
        }

        [Fact]
        public async Task DownloadInstallerExistingVersionNewer()
        {
            Directory.CreateDirectory(testDir);
            File.WriteAllText(testFilePath, "Newer installer content");
            string existingVersion = "3.0.0";

            int versionComparison = utils.CompareVersions(existingVersion, "2.0.0");
            bool shouldDownload = versionComparison < 0;
            bool result = false;
            if (shouldDownload)
                result = await mainCode.DownloadInstaller($"{server.Urls[0]}/testInstaller.exe", "testInstaller", "2.0.0");
            else
                Assert.Fail();
            Assert.False(result);
            Assert.True(File.Exists(testFilePath));
        }

        private void ResetDatabase()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM dbo.telemetryData", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public void IncreaseTelemetryDataValidId()
        {
            ResetDatabase();

            using(SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand("INSERT INTO dbo.telemetryData VALUES (1,0)", con))
                {
                    cmd.ExecuteNonQuery();
                }

                var databaseService = new DatabaseService(connStr);
                var code = new MainCode(connStr, databaseService, null);

                int rowsAffected = code.IncreaseTelemetryData(1);

                Assert.Equal(1, rowsAffected);
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT downloadCount FROM dbo.telemetryData WHERE telemetryId = @id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", 1);
                    int newCount = (int)cmd.ExecuteScalar();
                    Assert.Equal(1, newCount);
                }
            }
        }

        [Fact]
        public void IncreaseTelemetryDataInvalidId()
        {
            ResetDatabase();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.telemetryData VALUES (1,0)", con))
                {
                    cmd.ExecuteScalar();
                }
            }

            var databaseService = new DatabaseService(connStr);
            var telemetryService = new MainCode(connStr, databaseService, null);

            int rowsAffected = telemetryService.IncreaseTelemetryData(123); //Nieistniej¹ce ID

            Assert.Equal(0, rowsAffected);
        }

        [Fact]
        public void FindAppSuccess()
        {
            string testKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TestProgramSoftwareManager8713";
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                key.SetValue("DisplayName", "TestProgramSoftwareManager8713");
                key.SetValue("DisplayVersion", "1.0");
            }

            ObservableCollection<InstalledApplications> installed = new ObservableCollection<InstalledApplications>();

            utils.ListPrograms(installed);

            Assert.NotNull(InstalledApplications.FindApp("TestProgramSoftwareManager8713", installed));

            Registry.LocalMachine.DeleteSubKey(testKeyPath);
        }

        [Fact]
        public void FindAppFailure()
        {
            string testKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TestProgramSoftwareManager8713";
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                key.SetValue("DisplayName", "TestProgramSoftwareManager8713");
                key.SetValue("DisplayVersion", "1.0");
            }

            ObservableCollection<InstalledApplications> installed = new ObservableCollection<InstalledApplications>();

            utils.ListPrograms(installed);

            Assert.Null(InstalledApplications.FindApp("TestProgramSoftwareManager9682", installed));

            Registry.LocalMachine.DeleteSubKey(testKeyPath);
        }
    }
}