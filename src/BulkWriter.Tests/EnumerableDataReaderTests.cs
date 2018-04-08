using System;
using System.Collections.Generic;
using System.Linq;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    public class EnumerableDataReaderTests
    {
        private readonly IEnumerable<MyTestClass> _enumerable;
        private readonly EnumerableDataReader<MyTestClass> _dataReader;

        public EnumerableDataReaderTests()
        {
            _enumerable = new[] { new MyTestClass() };

            var propertyMappings = typeof(MyTestClass).BuildMappings();

            _dataReader = new EnumerableDataReader<MyTestClass>(_enumerable, propertyMappings);
            _dataReader.Read();
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
        public void GetString_Returns_Correct_Value()
        {
            var element = _enumerable.ElementAt(0);
            element.Name = "Michael";

            Assert.Equal("Michael", _dataReader.GetString(1));
        }

        [Fact]
        public void GetBytes_Returns_Correct_Value_Equal_Buffer()
        {
            var element = _enumerable.ElementAt(0);
            element.Data = new byte[16];
            new Random().NextBytes(element.Data);

            var buffer = new byte[16];
            var count = _dataReader.GetBytes(2, 0, buffer, 0, 0);

            Assert.Equal(16, count);
            Assert.Equal(element.Data, buffer);
        }

        [Fact]
        public void GetBytes_Returns_Correct_Value_Less_Than_Buffer()
        {
            var element = _enumerable.ElementAt(0);
            element.Data = new byte[16];
            new Random().NextBytes(element.Data);

            var buffer = new byte[32];
            var count = _dataReader.GetBytes(2, 0, buffer, 0, 0);

            Assert.Equal(16, count);
            Assert.Equal(element.Data, buffer.Take(16));
            Assert.True(buffer.Skip(16).Take(16).All(b => b == 0));
        }

        [Fact]
        public void GetBytes_Returns_Correct_Value_Greater_Than_Buffer_Partial_Page()
        {
            var element = _enumerable.ElementAt(0);
            element.Data = new byte[24];
            new Random().NextBytes(element.Data);

            var buffer = new byte[16];
            var result = new byte[24];
            var count = ByteReadHelper(2, buffer, result);

            Assert.Equal(24, count);
            Assert.Equal(element.Data, result);
        }

        [Fact]
        public void GetBytes_Returns_Correct_Value_Greater_Than_Buffer_Multiple_Full_Pages()
        {
            var element = _enumerable.ElementAt(0);
            element.Data = new byte[16 * 3];
            new Random().NextBytes(element.Data);

            var buffer = new byte[16];
            var result = new byte[16 * 3];
            var count = ByteReadHelper(2, buffer, result);

            Assert.Equal(16 * 3, count);
            Assert.Equal(element.Data, result);
        }

        [Fact]
        public void GetBytes_Returns_Correct_Value_Empty()
        {
            var element = _enumerable.ElementAt(0);
            element.Data = new byte[0];
            new Random().NextBytes(element.Data);

            var buffer = new byte[16];
            var result = new byte[0];
            var count = ByteReadHelper(2, buffer, result);

            Assert.Equal(0, count);
            Assert.Equal(element.Data, result);
        }

        [Fact]
        public void GetBytes_Returns_Correct_Value_Default_BulkCopy_Buffer()
        {
            var element = _enumerable.ElementAt(0);
            element.Data = Guid.NewGuid().ToByteArray();

            var buffer = new byte[4096];
            var result = new byte[element.Data.Length];
            var count = ByteReadHelper(2, buffer, result);

            Assert.Equal(element.Data.Length, count);
            Assert.Equal(element.Data, result);
        }

        private long ByteReadHelper(int ordinal, byte[] buffer, byte[] result)
        {
            long count;
            long offset = 0;
            do
            {
                count = _dataReader.GetBytes(ordinal, offset, buffer, 0, 0);
                Array.Copy(buffer, 0, result, offset, count);
                offset += count;
            } while (count == buffer.Length);
            return offset;
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
            Assert.Equal(3, _dataReader.FieldCount);
        }



        public class MyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public byte[] Data { get; set; }
        }
    }
}