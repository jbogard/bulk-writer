using System;
using System.Collections.Generic;
using System.Linq;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class EnumerableDataReaderTests : IDisposable
    {
        string connectionString = TestHelpers.ConnectionString;

        private readonly string tableName = BulkWriter.Internal.AutoDiscover.TableName<MyTestClass>(false);

        private IEnumerable<MyTestClass> enumerable;
        private EnumerableDataReader<MyTestClass> dataReader;
        
        public EnumerableDataReaderTests()
        {
            this.enumerable = new[] { new MyTestClass() };

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder.MapAllProperties<MyTestClass>();
            var propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();
            BulkWriter.Internal.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            this.dataReader = new EnumerableDataReader<MyTestClass>(enumerable, propertyMappings);
            dataReader.Read();
        }

        public void Dispose()
        {
            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);
        }

        [Fact]
        public void Read_Advances_Enumerable()
        {
            Assert.Same(enumerable.ElementAt(0), dataReader.Current);
        }

        [Fact]
        public void GetOrdinal_Returns_Correct_Value()
        {
            Assert.Equal(0, dataReader.GetOrdinal("Id"));
        }

        [Fact]
        public void IsDbNull_Returns_Correct_Value()
        {
            Assert.True(this.dataReader.IsDBNull(1));
        }

        [Fact]
        public void GetValue_Returns_Correct_Value()
        {
            var element = this.enumerable.ElementAt(0);
            element.Id = 418;
            element.Name = "Michael";

            Assert.Equal(418, this.dataReader.GetValue(0));
            Assert.Equal("Michael", this.dataReader.GetValue(1));
        }

        [Fact]
        public void GetName_Returns_Correct_Value()
        {
            Assert.Equal("Id", this.dataReader.GetName(0));
            Assert.Equal("Name", this.dataReader.GetName(1));
        }

        [Fact]
        public void FieldCount_Returns_Correct_Value()
        {
            Assert.Equal(2, this.dataReader.FieldCount);
        }
        
        public class MyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}