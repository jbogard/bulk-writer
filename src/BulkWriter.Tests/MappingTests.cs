using System;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class MappingTests
    {
        [Fact]
        public void Cannot_Read_Property_Without_Set()
        {
            var mappingDestination = new Internal.MappingDestination();
            string columnName = null;
            Assert.Throws<InvalidOperationException>(() => columnName = mappingDestination.ColumnName);
        }

        [Fact]
        public void Property_Reports_As_Set()
        {
            var destination = new Internal.MappingDestination {ColumnName = "TestColumn"};

            Assert.True(destination.IsPropertySet(Internal.MappingProperty.ColumnName));
            Assert.Equal("TestColumn", destination.ColumnName);

            var random = new Random();

            var columnOrdinal = random.Next();
            destination.ColumnOrdinal = columnOrdinal;
            Assert.True(destination.IsPropertySet(Internal.MappingProperty.ColumnOrdinal));
            Assert.Equal(destination.ColumnOrdinal, columnOrdinal);

            var columnSize = random.Next();
            destination.ColumnSize = columnSize;
            Assert.True(destination.IsPropertySet(Internal.MappingProperty.ColumnSize));
            Assert.Equal(destination.ColumnSize, columnSize);

            destination.DataTypeName = "TestDataTypeName";
            Assert.True(destination.IsPropertySet(Internal.MappingProperty.DataTypeName));
            Assert.Equal("TestDataTypeName", destination.DataTypeName);

            destination.IsKey = true;
            Assert.True(destination.IsPropertySet(Internal.MappingProperty.IsKey));
            Assert.True(destination.IsKey);
        }
    }
}
