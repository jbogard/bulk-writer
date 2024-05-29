using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkWriter.Tests
{
    [Collection(nameof(DbContainerFixture))]
    public class BulkWriterInitializationTests
    {
        private readonly string _connectionString;

        public class BulkWriterInitializationTestsMyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
        
        private readonly DbContainerFixture _fixture;

        public BulkWriterInitializationTests(DbContainerFixture fixture)
        {
            _fixture = fixture;
            _connectionString = fixture.TestConnectionString;
        }

        [Table("TestClass2")]
        public class BulkWriterInitializationTestsMyTestClassAnnotation
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public async Task CanSetBulkCopyParameters()
        {
            string tableName = DropCreate(nameof(BulkWriterInitializationTestsMyTestClass));

            const int timeout = 10;
            const int batchSize = 1000;
            bool setupCallbackInvoked = false;

            var writer = new BulkWriter<BulkWriterInitializationTestsMyTestClass>(_connectionString)
            {
                BulkCopyTimeout = timeout,
                BatchSize = batchSize,
                BulkCopySetup = bcp =>
                {
                    setupCallbackInvoked = true;
                    Assert.Equal(nameof(BulkWriterInitializationTestsMyTestClass), bcp.DestinationTableName);
                    Assert.Equal(timeout, bcp.BulkCopyTimeout);
                    Assert.Equal(batchSize, bcp.BatchSize);
                }
            };

            var items = Enumerable.Range(1, 10)
                .Select(i => new BulkWriterInitializationTestsMyTestClass { Id = i, Name = "Bob" });

            writer.WriteToDatabase(items);

            var count = (int)await _fixture.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");

            Assert.Equal(10, count);
            Assert.True(setupCallbackInvoked);
        }

        [Fact]
        public async Task CanSetBulkCopyParameters_Respects_Table_Annotation()
        {
            string tableName = DropCreate("TestClass2");
            const int timeout = 10;
            const int batchSize = 1000;
            bool setupCallbackInvoked = false;

            var writer = new BulkWriter<BulkWriterInitializationTestsMyTestClassAnnotation>(_connectionString)
            {
                BulkCopyTimeout = timeout,
                BatchSize = batchSize,
                BulkCopySetup = bcp =>
                {
                    setupCallbackInvoked = true;
                    Assert.Equal("TestClass2", bcp.DestinationTableName);
                    Assert.Equal(timeout, bcp.BulkCopyTimeout);
                    Assert.Equal(batchSize, bcp.BatchSize);
                }
            };

            var items = Enumerable.Range(1, 10)
                .Select(i => new BulkWriterInitializationTestsMyTestClassAnnotation { Id = i, Name = "Bob" });

            writer.WriteToDatabase(items);

            var count = (int)await _fixture.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName }");

            Assert.Equal(10, count);
            Assert.True(setupCallbackInvoked);
        }

        private string DropCreate(string tableName)
        {
            _fixture.ExecuteNonQuery(_connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            _fixture.ExecuteNonQuery(_connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            return tableName;
        }
    }
}
