using System;
using BulkWriter;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class MappingTests
    {
        [Fact]
        public void Cannot_Read_Property_Without_Set()
        {
            var mappingDestination = new BulkWriter.Internal.MappingDestination();
            string columnName = null;
            Assert.Throws<InvalidOperationException>(() => columnName = mappingDestination.ColumnName);
        }

        [Fact]
        public void Property_Reports_As_Set()
        {
            var destination = new BulkWriter.Internal.MappingDestination();

            destination.ColumnName = "TestColumn";
            Assert.True(destination.IsPropertySet(BulkWriter.Internal.MappingProperty.ColumnName));
            Assert.Equal(destination.ColumnName, "TestColumn");

            var random = new Random();

            int columnOrdinal = random.Next();
            destination.ColumnOrdinal = columnOrdinal;
            Assert.True(destination.IsPropertySet(BulkWriter.Internal.MappingProperty.ColumnOrdinal));
            Assert.Equal(destination.ColumnOrdinal, columnOrdinal);

            int columnSize = random.Next();
            destination.ColumnSize = columnSize;
            Assert.True(destination.IsPropertySet(BulkWriter.Internal.MappingProperty.ColumnSize));
            Assert.Equal(destination.ColumnSize, columnSize);

            destination.DataTypeName = "TestDataTypeName";
            Assert.True(destination.IsPropertySet(BulkWriter.Internal.MappingProperty.DataTypeName));
            Assert.Equal(destination.DataTypeName, "TestDataTypeName");

            destination.IsKey = true;
            Assert.True(destination.IsPropertySet(BulkWriter.Internal.MappingProperty.IsKey));
            Assert.Equal(destination.IsKey, true);
        }
    }
}
