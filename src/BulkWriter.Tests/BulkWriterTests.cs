using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkWriter.Tests
{
    [Collection(nameof(DbContainerFixture))]
    public class BulkWriterTests
    {
        private readonly DbContainerFixture _fixture;

        public BulkWriterTests(DbContainerFixture fixture) => _fixture = fixture;

        public class BulkWriterTestsMyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class BulkWriterTestsMyTestClassWithKey
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public async Task CanWriteSync()
        {
            string tableName = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClass));

            var writer = new BulkWriter<BulkWriterTestsMyTestClass>(_fixture.TestConnectionString);

            var items = Enumerable.Range(1, 1000).Select(i => new BulkWriterTestsMyTestClass { Id = i, Name = "Bob" });

            writer.WriteToDatabase(items);

            var count = (int)await _fixture.ExecuteScalar($"SELECT COUNT(1) FROM {tableName}");

            Assert.Equal(1000, count);
        }

        [Fact]
        public async Task CanWriteSyncWithOptions()
        {
            var tableName = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClass));
            var tableNameWithKey = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClassWithKey));

            var writer = new BulkWriter<BulkWriterTestsMyTestClass>(_fixture.TestConnectionString);
            var writerWithOptions = new BulkWriter<BulkWriterTestsMyTestClassWithKey>(_fixture.TestConnectionString, SqlBulkCopyOptions.KeepIdentity);

            var items = Enumerable.Range(11, 20).Select(i => new BulkWriterTestsMyTestClass { Id = i, Name = "Bob" });
            var itemsWithKey = Enumerable.Range(11, 20).Select(i => new BulkWriterTestsMyTestClassWithKey { Id = i, Name = "Bob" });

            writer.WriteToDatabase(items);
            writerWithOptions.WriteToDatabase(itemsWithKey);

            var minId = (int)await _fixture.ExecuteScalar($"SELECT MIN(Id) FROM {tableName}");
            var minIdWithKey = (int)await _fixture.ExecuteScalar($"SELECT MIN(Id) FROM {tableNameWithKey}");

            Assert.Equal(1, minId);
            Assert.Equal(11, minIdWithKey);
        }

        [Fact]
        public async Task CanWriteSyncWithExistingConnection()
        {
            string tableName = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClass));

            using (var connection = new SqlConnection(_fixture.TestConnectionString))
            {
                await connection.OpenAsync();

                var writer = new BulkWriter<BulkWriterTestsMyTestClass>(connection);

                var items = Enumerable.Range(1, 1000)
                    .Select(i => new BulkWriterTestsMyTestClass { Id = i, Name = "Bob" });

                writer.WriteToDatabase(items);

                var count = (int)await _fixture.ExecuteScalar(connection, $"SELECT COUNT(1) FROM {tableName}");

                Assert.Equal(1000, count);
            }
        }

        [Fact]
        public async Task CanWriteSyncWithExistingConnectionAndTransaction()
        {
            string tableName = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClass));

            using (var connection = new SqlConnection(_fixture.TestConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {

                    var writer = new BulkWriter<BulkWriterTestsMyTestClass>(connection, transaction);

                    var items = Enumerable.Range(1, 1000)
                        .Select(i => new BulkWriterTestsMyTestClass { Id = i, Name = "Bob" });

                    writer.WriteToDatabase(items);

                    var count = (int)await _fixture.ExecuteScalar(connection, $"SELECT COUNT(1) FROM {tableName}", transaction);

                    Assert.Equal(1000, count);

                    transaction.Rollback();

                    count = (int)await _fixture.ExecuteScalar(connection, $"SELECT COUNT(1) FROM {tableName}");

                    Assert.Equal(0, count);
                }
            }
        }

        [Fact]
        public async Task CanWriteSyncWithExistingConnectionAndTransactionAndOptions()
        {
            var tableName = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClass));
            var tableNameWithKey = _fixture.DropCreate(nameof(BulkWriterTestsMyTestClassWithKey));

            using (var connection = new SqlConnection(_fixture.TestConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    var writer = new BulkWriter<BulkWriterTestsMyTestClass>(connection, transaction);
                    var writerWithOptions = new BulkWriter<BulkWriterTestsMyTestClassWithKey>(connection, SqlBulkCopyOptions.KeepIdentity, transaction);

                    var items = Enumerable.Range(11, 20).Select(i => new BulkWriterTestsMyTestClass { Id = i, Name = "Bob" });
                    var itemsWithKey = Enumerable.Range(11, 20).Select(i => new BulkWriterTestsMyTestClassWithKey { Id = i, Name = "Bob" });

                    writer.WriteToDatabase(items);
                    writerWithOptions.WriteToDatabase(itemsWithKey);

                    var minId = (int?)await _fixture.ExecuteScalar(connection, $"SELECT MIN(Id) FROM {tableName}", transaction);
                    var minIdWithKey = (int?)await _fixture.ExecuteScalar(connection, $"SELECT MIN(Id) FROM {tableNameWithKey}", transaction);

                    Assert.Equal(1, minId);
                    Assert.Equal(11, minIdWithKey);

                    transaction.Rollback();

                    var emptyMinId = await _fixture.ExecuteScalar(connection, $"SELECT MIN(Id) FROM {tableName}");
                    var emptyMinIdWithKey = await _fixture.ExecuteScalar(connection, $"SELECT MIN(Id) FROM {tableNameWithKey}");

                    Assert.Equal(emptyMinId, System.DBNull.Value);
                    Assert.Equal(emptyMinIdWithKey, System.DBNull.Value);
                }
            }
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
            string tableName = _fixture.DropCreate(nameof(OrdinalAndColumnNameExampleType));

            var writer = new BulkWriter<OrdinalAndColumnNameExampleType>(_fixture.TestConnectionString);

            var items = new[] { new OrdinalAndColumnNameExampleType { Id = 1, Name2 = "Bob" } };

            writer.WriteToDatabase(items);

            var count = (int)await _fixture.ExecuteScalar($"SELECT COUNT(1) FROM {tableName}");

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
            _fixture.ExecuteNonQuery($"DROP TABLE IF EXISTS [dbo].[{tableName}]");
            _fixture.ExecuteNonQuery(
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](MAX) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var writer = new BulkWriter<MyTestClassForNvarCharMax>(_fixture.TestConnectionString);

            var items = new[] { new MyTestClassForNvarCharMax { Id = 1, Name = "Bob" } };

            writer.WriteToDatabase(items);

            var count = (int)await _fixture.ExecuteScalar($"SELECT COUNT(1) FROM {tableName}");

            Assert.Equal(1, count);
        }
        
        public class MyTestClassForVarBinary
        {
            public int Id { get; set; }
            public byte[] Data { get; set; }
        }

        [Fact]
        public async Task Should_Handle_Column_VarBinary_Large()
        {
            string tableName = nameof(MyTestClassForVarBinary);

            _fixture.ExecuteNonQuery($"DROP TABLE IF EXISTS [dbo].[{tableName}]");
            _fixture.ExecuteNonQuery(
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Data] [varbinary](MAX) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var writer = new BulkWriter<MyTestClassForVarBinary>(_fixture.TestConnectionString);
            var items = new[] { new MyTestClassForVarBinary { Id = 1, Data = new byte[1024 * 1024 * 1] } };
            new Random().NextBytes(items.First().Data);

            writer.WriteToDatabase(items);

            var count = (int)await _fixture.ExecuteScalar($"SELECT COUNT(1) FROM {tableName}");
            var data = (byte[])await _fixture.ExecuteScalar($"SELECT TOP 1 Data FROM {tableName}");
            Assert.Equal(items.First().Data, data);
            Assert.Equal(1, count);
        }
    }
}
