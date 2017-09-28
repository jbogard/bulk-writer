using System.Collections.Generic;
using System.Data.SqlClient;

namespace BulkWriter.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SetupDb();

            using (var bulkWriter = new BulkWriter<MyDomainEntity>(@"Data Source=(localdb)\mssqllocaldb;Database=BulkWriter.Demo;Trusted_Connection=True;"))
            {
                var items = GetDomainEntities();
                bulkWriter.WriteToDatabase(items);
            }
        }

        private static void SetupDb()
        {
            using (var sqlConnection = new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Trusted_Connection=True;"))
            {
                sqlConnection.Open();
                using (var command = new SqlCommand(
                    @"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'BulkWriter.Demo')
CREATE DATABASE [BulkWriter.Demo]", sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SqlCommand(@"USE [BulkWriter.Demo]; DROP TABLE IF EXISTS dbo.MyDomainEntities", sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SqlCommand(@"USE [BulkWriter.Demo]; CREATE TABLE dbo.MyDomainEntities (
    [Id] [int] IDENTITY(1, 1) NOT NULL,
    [FirstName] [nvarchar](100),
    [LastName] [nvarchar](100),
    CONSTRAINT [PK_MyDomainEntities] PRIMARY KEY CLUSTERED ( [Id] ASC )
)", sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static IEnumerable<MyDomainEntity> GetDomainEntities()
        {
            for (var i = 0; i < 1000; i++)
            {
                yield return new MyDomainEntity
                {
                    Id = i,
                    FirstName = $"Bob-{i}",
                    LastName = $"Smith-{i}"
                };
            }
        }
    }
}