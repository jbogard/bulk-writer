using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BulkWriter.Tests
{
    internal static class TestHelpers
    {
        static TestHelpers()
        {
            ConnectionString = @"Data Source=(localdb)\mssqllocaldb;Database=BulkWriter.Tests;Trusted_Connection=True;";

            string admin = @"Data Source=(localdb)\mssqllocaldb;Trusted_Connection=True;";
            ExecuteNonQuery(admin, @"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'BulkWriter.Tests')
CREATE DATABASE [BulkWriter.Tests]");
        }


        public static void ExecuteNonQuery(string connectionString, string commandText)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(commandText, sqlConnection))
                {
                    sqlConnection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static async Task<object> ExecuteScalar(string connectionString, string commandText)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(commandText, sqlConnection))
                {
                    sqlConnection.Open();
                    return await command.ExecuteScalarAsync();
                }
            }
        }

        public static string ConnectionString { get; }
    }
}