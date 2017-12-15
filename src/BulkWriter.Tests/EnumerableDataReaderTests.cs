using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class EnumerableDataReaderTests : IDisposable
    {
        private readonly string _connectionString = TestHelpers.ConnectionString;

        private readonly string _tableName = nameof(MyTestClass);

        private readonly IEnumerable<MyTestClass> _enumerable;
        private readonly EnumerableDataReader<MyTestClass> _dataReader;

        private readonly IEnumerable<MyCustomOrdinalsTestClass> _customOrdinalsEnumerable;
        private readonly EnumerableDataReader<MyCustomOrdinalsTestClass> _customOrdinalsDataReader;
        
        public EnumerableDataReaderTests()
        {
            _enumerable = new[] { new MyTestClass() };
            _customOrdinalsEnumerable = new[] { new MyCustomOrdinalsTestClass() };

            TestHelpers.ExecuteNonQuery(_connectionString, $"DROP TABLE IF EXISTS [dbo].[{_tableName}]");

            TestHelpers.ExecuteNonQuery(_connectionString,
                "CREATE TABLE [dbo].[" + _tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + _tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var propertyMappings = typeof(MyTestClass).BuildMappings();

            _dataReader = new EnumerableDataReader<MyTestClass>(_enumerable, propertyMappings);
            _dataReader.Read();


            var customOrdinalPropertyMappings = typeof(MyCustomOrdinalsTestClass).BuildMappings();
            _customOrdinalsDataReader = new EnumerableDataReader<MyCustomOrdinalsTestClass>(_customOrdinalsEnumerable, customOrdinalPropertyMappings);
            _customOrdinalsDataReader.Read();
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
            Assert.Equal(1, _dataReader.GetOrdinal("Name"));
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


        [Fact]
        public void CustomOrdrinals_GetOrdinal_Returns_Correct_Value()
        {
            Assert.Equal(0, _customOrdinalsDataReader.GetOrdinal("Id"));
            Assert.Equal(1, _customOrdinalsDataReader.GetOrdinal("MiddleName"));
            Assert.Equal(2, _customOrdinalsDataReader.GetOrdinal("LastName"));
            Assert.Equal(3, _customOrdinalsDataReader.GetOrdinal("FirstName"));
        }

        public class MyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class MyCustomOrdinalsTestClass
        {
            [Column(Order = 0)]
            public int Id { get; set; }

            [Column(Order = 3)]
            public string FirstName { get; set; }

            [Column(Order = 1)]
            public string MiddleName { get; set; }

            [Column(Order = 2)]
            public string LastName { get; set; }
        }
    }
}