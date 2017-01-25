using System.Data.SqlClient;

namespace BulkWriterTests
{
    internal static class TestHelpers
    {
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
    }
}