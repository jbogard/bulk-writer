using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BulkWriter.Demo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            SetupDb();

            var timer = new Stopwatch();
            using (var bulkWriter = new BulkWriter<MyDomainEntity>(@"Data Source=.\sqlexpress;Database=BulkWriter.Demo;Trusted_Connection=True;Connection Timeout=300")
            {
                BulkCopyTimeout = 0,
                BatchSize = 10000
            })
            {
                var items = GetDomainEntities();
                timer.Start();
                await bulkWriter.WriteToDatabaseAsync(items);
                timer.Stop();
            }

            Console.WriteLine(timer.ElapsedMilliseconds);
            Console.ReadKey();
        }

        private static void SetupDb()
        {
            using (var sqlConnection = new SqlConnection(@"Data Source=.\sqlexpress;Trusted_Connection=True;"))
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
            for (var i = 0; i < 10000000; i++)
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