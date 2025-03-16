using Microsoft.Data.SqlClient;
using PracaInzynierska;
using Xunit;

namespace IntegrationTesting
{
    public class IntegrationTest1
    {
        private readonly string connStr = "server=localhost;database=testing;integrated Security=True;TrustServerCertificate=true;";

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
        public void IncreaseTelemetryData_ShouldIncrementDownloadCount()
        {
            // Arrange
            ResetDatabase();

            using(SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand("INSERT INTO dbo.telemetryData VALUES (1,0)", con))
                {
                    cmd.ExecuteScalar();
                }
                
            }

            int testId;
            using (SqlConnection con = new SqlConnection(connStr))
            {

                var databaseService = new DatabaseService(connStr);

                var telemetryService = new MainCode(connStr, databaseService, null);

                // Act
                int rowsAffected = telemetryService.IncreaseTelemetryData(1);

                // Assert
                Assert.Equal(1, rowsAffected);
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT downloadCount FROM dbo.telemetryData WHERE telemetryId = @id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", 1);
                    int newCount = (int)cmd.ExecuteScalar();
                    Assert.Equal(1, newCount);  // Download count should increase by 1
                }
            }
        }
    }
}