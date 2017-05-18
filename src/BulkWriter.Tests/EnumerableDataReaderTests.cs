using System;
using System.Collections.Generic;
using System.Linq;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class EnumerableDataReaderTests : IDisposable
    {
        private readonly string _connectionString = TestHelpers.ConnectionString;

        private readonly string _tableName = AutoDiscover.TableName<MyTestClass>(false);

        private readonly IEnumerable<MyTestClass> _enumerable;
        private readonly EnumerableDataReader<MyTestClass> _dataReader;
        
        public EnumerableDataReaderTests()
        {
            _enumerable = new[] { new MyTestClass() };

            TestHelpers.ExecuteNonQuery(_connectionString,
                "CREATE TABLE [dbo].[" + _tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + _tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder.MapAllProperties<MyTestClass>();
            var propertyMappings = ((MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();
            AutoDiscover.Mappings(_connectionString, _tableName, propertyMappings);

            _dataReader = new EnumerableDataReader<MyTestClass>(_enumerable, propertyMappings);
            _dataReader.Read();
        }

        public void Dispose()
        {
            TestHelpers.ExecuteNonQuery(_connectionString, "DROP TABLE " + _tableName);
        }

        [Fact]
        public void Read_Advances_Enumerable()
        {
            Assert.Same(_enumerable.ElementAt(0), _dataReader.Current);
        }

        [Fact]
        public void GetOrdinal_Returns_Correct_Value()
        {
            Assert.Equal(0, _dataReader.GetOrdinal("Id"));
        }

        [Fact]
        public void IsDbNull_Returns_Correct_Value()
        {
            Assert.True(_dataReader.IsDBNull(1));
        }

        [Fact]
        public void GetValue_Returns_Correct_Value()
        {
            var element = _enumerable.ElementAt(0);
            element.Id = 418;
            element.Name = "Michael";

            Assert.Equal(418, _dataReader.GetValue(0));
            Assert.Equal("Michael", _dataReader.GetValue(1));
        }

        [Fact]
        public void GetName_Returns_Correct_Value()
        {
            Assert.Equal("Id", _dataReader.GetName(0));
            Assert.Equal("Name", _dataReader.GetName(1));
        }

        [Fact]
        public void FieldCount_Returns_Correct_Value()
        {
            Assert.Equal(2, _dataReader.FieldCount);
        }
        
        public class MyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}