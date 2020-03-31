using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace BulkWriter.Benchmark
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "No user input here")]
    internal static class DbHelpers
    {
        private const string SetupConnectionString = "Data Source=.\\;Trusted_Connection=True;";

        private const string ConnectionString = "Data Source=.\\;Trusted_Connection=True;Initial Catalog=BulkWriter.Benchmark";
        private const string DbName = "BulkWriter.Benchmark";

        internal static SqlConnection OpenSqlConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            return connection;
        }

        internal static void SetupDb()
        {
            using var sqlConnection = new SqlConnection(SetupConnectionString);
            sqlConnection.Open();

            CreateDatabase(sqlConnection);
            SetSimpleRecovery(sqlConnection);

            DropTable<DomainEntity>(sqlConnection);
            CreateDomainEntitiesTable(sqlConnection);
        }

        internal static void CreateDatabase(SqlConnection sqlConnection)
        {
            var createDatabaseSql = @$"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{DbName}')
CREATE DATABASE [{DbName}]";
            using var command = new SqlCommand(createDatabaseSql, sqlConnection);
            command.ExecuteNonQuery();

            SetDatabaseSize(sqlConnection);
        }

        internal static void SetSimpleRecovery(SqlConnection sqlConnection)
        {
            var alterDatabaseSql = $"ALTER DATABASE [{DbName}] SET RECOVERY SIMPLE";
            using var command = new SqlCommand(alterDatabaseSql, sqlConnection);
            command.ExecuteNonQuery();
        }

        internal static void SetDatabaseSize(SqlConnection sqlConnection)
        {
            var alterDatabaseSql = @$"IF NOT EXISTS(SELECT name FROM [{DbName}].sys.database_files WHERE [type]=0 AND size > 100000)
ALTER DATABASE [{DbName}] MODIFY FILE (Name='{DbName}', SIZE = 1000MB)";
            using var command = new SqlCommand(alterDatabaseSql, sqlConnection);
            command.ExecuteNonQuery();
        }

        internal static void CreateDomainEntitiesTable(SqlConnection sqlConnection)
        {
            var tableName = GetTableName<DomainEntity>();
            var createTableSql = @$"USE [{DbName}]; CREATE TABLE dbo.{tableName} (
    [Id] [bigint] NOT NULL,
    [FirstName] [nvarchar](100),
    [LastName] [nvarchar](100),
    CONSTRAINT [PK_{1}] PRIMARY KEY CLUSTERED ([Id] ASC )
)";

            using var command = new SqlCommand(createTableSql, sqlConnection);
            command.ExecuteNonQuery();
        }

        internal static void DropTable<TEntity>(SqlConnection sqlConnection)
        {
            var tableName = GetTableName<TEntity>();
            var dropTableSql = $"USE [{DbName}]; DROP TABLE IF EXISTS {tableName}";
            using var command = new SqlCommand(dropTableSql, sqlConnection);
            command.ExecuteNonQuery();
        }

        internal static void TruncateTable<TEntity>(SqlConnection sqlConnection)
        {
            var tableName = GetTableName<TEntity>();
            var dropTableSql = $"USE [{DbName}]; TRUNCATE TABLE {tableName}";
            using var command = new SqlCommand(dropTableSql, sqlConnection);
            command.ExecuteNonQuery();
        }

        internal static string GetTableName<TEntity>()
        {
            var t = typeof(TEntity);
            var tableNameAttribute = t.GetCustomAttribute<TableAttribute>();
            return tableNameAttribute != null ? tableNameAttribute.Name : t.Name;
        }
    }
}
