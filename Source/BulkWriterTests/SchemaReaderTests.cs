using System.IO;
using System.Linq;
using Headspring.BulkWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BulkWriterTests
{
    [TestClass]
    public class SchemaReaderTests
    {
        [TestMethod]
        public void Can_Read_Table_Schema()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
            const string tableName = "TempTestTable";
            
            const string createTableScript = "CREATE TABLE [dbo].[" + tableName + "](" +
                                             "[Id] [int] IDENTITY(1,1) NOT NULL," +
                                             "[FirstName] [nvarchar](50) NULL," +
                                             "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                                             ")";
            
            const string dropTableScript = "drop table [" + tableName + "]";

            TestHelpers.ExecuteNonQuery(connectionString, createTableScript);

            var schemaRows = SchemaReader.GetSortedSchemaRows(connectionString, tableName);

            TestHelpers.ExecuteNonQuery(connectionString, dropTableScript);

            var idRow = schemaRows.ElementAtOrDefault(0);
            Assert.IsNotNull(idRow);
            Assert.AreEqual("Id", idRow.ColumnName);
            Assert.AreEqual(typeof (int), idRow.DataType);
            Assert.AreEqual("int", idRow.DataTypeName);
            Assert.IsTrue(idRow.IsKey);
            Assert.IsFalse(idRow.AllowDbNull);

            var firstNameRow = schemaRows.ElementAtOrDefault(1);
            Assert.IsNotNull(firstNameRow);
            Assert.AreEqual("FirstName", firstNameRow.ColumnName);
            Assert.AreEqual(50, firstNameRow.Size);
            Assert.AreEqual("nvarchar", firstNameRow.DataTypeName);
            Assert.AreEqual(typeof(string), firstNameRow.DataType);
            Assert.IsTrue(firstNameRow.AllowDbNull);
        }
    }
}