using System.IO;
using System.Linq;
using BulkWriter;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class SchemaReaderTests
    {
        [Fact]
        public void Can_Read_Table_Schema()
        {
            string connectionString = TestHelpers.ConnectionString;
            const string tableName = "TempTestTable";
            
            const string createTableScript = "CREATE TABLE [dbo].[" + tableName + "](" +
                                             "[Id] [int] IDENTITY(1,1) NOT NULL," +
                                             "[FirstName] [nvarchar](50) NULL," +
                                             "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                                             ")";
            
            const string dropTableScript = "drop table [" + tableName + "]";

            TestHelpers.ExecuteNonQuery(connectionString, createTableScript);

            var schemaRows = BulkWriter.SchemaReader.GetSortedSchemaRows(connectionString, tableName);

            TestHelpers.ExecuteNonQuery(connectionString, dropTableScript);

            var idRow = schemaRows.ElementAtOrDefault(0);
            Assert.NotNull(idRow);
            Assert.Equal("Id", idRow.ColumnName);
            Assert.Equal(typeof (int), idRow.DataType);
            Assert.Equal("int", idRow.DataTypeName);
            Assert.True(idRow.IsKey);
            Assert.False(idRow.AllowDbNull);

            var firstNameRow = schemaRows.ElementAtOrDefault(1);
            Assert.NotNull(firstNameRow);
            Assert.Equal("FirstName", firstNameRow.ColumnName);
            Assert.Equal(50, firstNameRow.Size);
            Assert.Equal("nvarchar", firstNameRow.DataTypeName);
            Assert.Equal(typeof(string), firstNameRow.DataType);
            Assert.True(firstNameRow.AllowDbNull);
        }
    }
}