using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Data.SqlClient;

namespace BulkWriter.Benchmark.Benchmarks
{
    public class BulkWriterBenchmark : BenchmarkBaseClass
    {
        [Benchmark]
        public async Task BulkWriter()
        {
            await using var sqlConnection = DbHelpers.OpenSqlConnection();
            using var bulkWriter = new BulkWriter<DomainEntity>(sqlConnection)
            {
                BulkCopyTimeout = 0,
                BatchSize = 10000
            };

            var items = GetTestRecords();
            await bulkWriter.WriteToDatabaseAsync(items);
        }

        [Benchmark(Baseline = true)]
        public async Task OneRecordAtATime()
        {
            var tableName = DbHelpers.GetTableName<DomainEntity>();
            var insertSql = $"INSERT INTO {tableName} (Id, FirstName, LastName) VALUES (@Id, @FirstName, @LastName)";

            await using var sqlConnection = DbHelpers.OpenSqlConnection();

            var records = GetTestRecords();
            foreach (var domainEntity in records)
            {
                var sqlCommand = new SqlCommand(insertSql, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@Id", domainEntity.Id);
                sqlCommand.Parameters.AddWithValue("@FirstName", domainEntity.FirstName);
                sqlCommand.Parameters.AddWithValue("@LastName", domainEntity.LastName);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        [Benchmark]
        public async Task Batched100()
        {
            var tableName = DbHelpers.GetTableName<DomainEntity>();
            var insertSql = $"INSERT INTO {tableName} (Id, FirstName, LastName) VALUES ";

            await using var sqlConnection = DbHelpers.OpenSqlConnection();

            var batchSize = 100;
            var currentBatchSize = 0;
            var records = GetTestRecords();
            
            var queryBuilder = new StringBuilder(insertSql);
            var sqlCommand = new SqlCommand("", sqlConnection);

            foreach (var record in records)
            {
                queryBuilder.Append(currentBatchSize == 0
                    ? "(@p0, @p1, @p2)"
                    : $",(@p{currentBatchSize * 3}, @p{currentBatchSize * 3 + 1}, @p{currentBatchSize * 3 + 2})");

                sqlCommand.Parameters.AddWithValue($"@p{currentBatchSize * 3}", record.Id);
                sqlCommand.Parameters.AddWithValue($"@p{currentBatchSize * 3 + 1}", record.FirstName);
                sqlCommand.Parameters.AddWithValue($"@p{currentBatchSize * 3 + 2}", record.LastName);

                ++currentBatchSize;

                if (currentBatchSize == batchSize)
                {
                    sqlCommand.CommandText = queryBuilder.ToString();
                    await sqlCommand.ExecuteNonQueryAsync();

                    currentBatchSize = 0;

                    queryBuilder.Clear();
                    queryBuilder.Append(insertSql);

                    sqlCommand.CommandText = "";
                    sqlCommand.Parameters.Clear();
                }
            }

            if (currentBatchSize > 0)
            {
                sqlCommand.CommandText = queryBuilder.ToString();
                await sqlCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
