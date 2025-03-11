using Microsoft.Data.SqlClient;

namespace PracaInzynierska
{
    public interface IDatabaseService
    {
        int ExecuteNonQuery(string query);
        SqlDataReader ExecuteReader(string query);
    }
}