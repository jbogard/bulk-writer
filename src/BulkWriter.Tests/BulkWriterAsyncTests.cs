using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkWriter.Tests
{
    public class BulkWriterAsyncTests
    {
        private readonly string _connectionString = TestHelpers.ConnectionString;

        public class BulkWriterAsyncTestsMyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public async Task CanWriteAsync()
        {
            string tableName = DropCreate(nameof(BulkWriterTests.BulkWriterTestsMyTestClass));

            var writer = new BulkWriter<BulkWriterTests.BulkWriterTestsMyTestClass>(_connectionString);

            var items = Enumerable.Range(1, 1000).Select(i => new BulkWriterTests.BulkWriterTestsMyTestClass { Id = i, Name = "Bob" });

            await writer.WriteToDatabaseAsync(items);

            var count = (int)await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");

            Assert.Equal(1000, count);
        }

        public class OrdinalAndColumnNameExampleType
        {
            [NotMapped]
            public string Dummy { get; set; }

            [Column(Order = 0)]
            public int Id { get; set; }

            [NotMapped]
            public string Name { get; set; }

            [Column("Name")]
            public string Name2 { get; set; }
        }

        [Fact]
        public async Task Should_Handle_Both_Ordinal_And_ColumnName_For_Destination_Mapping()
        {
            string tableName = DropCreate(nameof(BulkWriterTests.OrdinalAndColumnNameExampleType));

            var writer = new BulkWriter<BulkWriterTests.OrdinalAndColumnNameExampleType>(_connectionString);

            var items = new[] { new BulkWriterTests.OrdinalAndColumnNameExampleType { Id = 1, Name2 = "Bob" } };

            await writer.WriteToDatabaseAsync(items);

            var count = (int)await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");

            Assert.Equal(1, count);
        }

        public class MyTestClassForNvarCharMax
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public async Task Should_Handle_Column_Nvarchar_With_Length_Max()
        {
            string tableName = nameof(MyTestClassForNvarCharMax);
            TestHelpers.ExecuteNonQuery(_connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");
            TestHelpers.ExecuteNonQuery(_connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](MAX) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var writer = new BulkWriter<MyTestClassForNvarCharMax>(_connectionString);

            var items = new[] { new MyTestClassForNvarCharMax { Id = 1, Name = "Bob" } };

            await writer.WriteToDatabaseAsync(items);

            var count = (int)await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");

            Assert.Equal(1, count);
        }


        private string DropCreate(string tableName)
        {
            TestHelpers.ExecuteNonQuery(_connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(_connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            return tableName;
        }
    }
}