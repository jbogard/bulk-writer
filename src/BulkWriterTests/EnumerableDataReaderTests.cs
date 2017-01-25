using System.Collections.Generic;
using System.Linq;
using BulkWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BulkWriterTests
{
    [TestClass]
    public class EnumerableDataReaderTests
    {
        const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";

        private readonly string tableName = AutoDiscover.TableName<MyTestClass>(false);

        private IEnumerable<MyTestClass> enumerable;
        private EnumerableDataReader<MyTestClass> dataReader;
        
        [TestInitialize]
        public void Initialize_Test()
        {
            this.enumerable = new[] { new MyTestClass() };

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder.MapAllProperties<MyTestClass>();
            var propertyMappings = ((MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();
            AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            this.dataReader = new EnumerableDataReader<MyTestClass>(enumerable, propertyMappings);
            dataReader.Read();
        }

        [TestCleanup]
        public void Cleanup_Test()
        {
            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);
        }

        [TestMethod]
        public void Read_Advances_Enumerable()
        {
            Assert.AreSame(enumerable.ElementAt(0), dataReader.Current);
        }

        [TestMethod]
        public void GetOrdinal_Returns_Correct_Value()
        {
            Assert.AreEqual(0, dataReader.GetOrdinal("Id"));
        }

        [TestMethod]
        public void IsDbNull_Returns_Correct_Value()
        {
            Assert.IsTrue(this.dataReader.IsDBNull(1));
        }

        [TestMethod]
        public void GetValue_Returns_Correct_Value()
        {
            var element = this.enumerable.ElementAt(0);
            element.Id = 418;
            element.Name = "Michael";

            Assert.AreEqual(418, this.dataReader.GetValue(0));
            Assert.AreEqual("Michael", this.dataReader.GetValue(1));
        }

        [TestMethod]
        public void GetName_Returns_Correct_Value()
        {
            Assert.AreEqual("Id", this.dataReader.GetName(0));
            Assert.AreEqual("Name", this.dataReader.GetName(1));
        }

        [TestMethod]
        public void FieldCount_Returns_Correct_Value()
        {
            Assert.AreEqual(2, this.dataReader.FieldCount);
        }
        
        public class MyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}