using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkWriter.Tests
{
    public class BulkWriterAsyncTests
    {
        private readonly string _connectionString = TestHelpers.ConnectionString;
        private readonly string _tableName = nameof(BulkWriterAsyncTestsMyTestClass);

        public class BulkWriterAsyncTestsMyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public BulkWriterAsyncTests()
        {
            TestHelpers.ExecuteNonQuery(_connectionString, $"DROP TABLE IF EXISTS [dbo].[{_tableName}]");

            TestHelpers.ExecuteNonQuery(_connectionString,
                "CREATE TABLE [dbo].[" + _tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + _tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");
        }

        [Fact]
        public async Task CanWriteSync()
        {
            var writer = new BulkWriter<BulkWriterAsyncTestsMyTestClass>(_connectionString);

            var items = Enumerable.Range(1, 1000).Select(i => new BulkWriterAsyncTestsMyTestClass { Id = i, Name = "Bob"});

            await writer.WriteToDatabaseAsync(items);

            var count = (int) await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {_tableName}");

            Assert.Equal(1000, count);
        }
    }
}