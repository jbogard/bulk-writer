using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Xunit;

namespace BulkWriter.Tests;


[CollectionDefinition(nameof(DbContainerFixture))]
public class DbContainerFixtureCollection : ICollectionFixture<DbContainerFixture> { }

public class DbContainerFixture : IAsyncLifetime
{
    public MsSqlContainer SqlContainer { get; } = new MsSqlBuilder().Build();
    public string TestConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        await SqlContainer.StartAsync();
            
        ExecuteNonQuery(SqlContainer.GetConnectionString(), @"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'BulkWriter.Tests')
                 CREATE DATABASE [BulkWriter.Tests]");

        var builder = new SqlConnectionStringBuilder(SqlContainer.GetConnectionString())
        {
            InitialCatalog = "BulkWriter.Tests"
        };

        TestConnectionString = builder.ToString();
    }

    public Task DisposeAsync()
        => SqlContainer.DisposeAsync().AsTask();
        
    public void ExecuteNonQuery(string commandText) => ExecuteNonQuery(TestConnectionString, commandText);

    public void ExecuteNonQuery(string connectionString, string commandText)
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

    public Task<object> ExecuteScalar(string commandText) => ExecuteScalar(TestConnectionString, commandText);

    public async Task<object> ExecuteScalar(string connectionString, string commandText)
    {
        using (var sqlConnection = new SqlConnection(connectionString))
        {
            await sqlConnection.OpenAsync();
            return await ExecuteScalar(sqlConnection, commandText);
        }
    }

    public async Task<object> ExecuteScalar(SqlConnection sqlConnection, string commandText, SqlTransaction transaction = null)
    {
        using (var command = new SqlCommand(commandText, sqlConnection, transaction))
        {
            return await command.ExecuteScalarAsync();
        }
    }

    public string DropCreate(string tableName)
    {
        ExecuteNonQuery(TestConnectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

        ExecuteNonQuery(TestConnectionString,
            "CREATE TABLE [dbo].[" + tableName + "](" +
            "[Id] [int] IDENTITY(1,1) NOT NULL," +
            "[Name] [nvarchar](50) NULL," +
            "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
            ")");

        return tableName;
    }

}
