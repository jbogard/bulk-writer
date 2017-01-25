using System;
using BulkWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BulkWriterTests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cannot_Read_Property_Without_Set()
        {
            var mappingDestination = new MappingDestination();
            string columnName = mappingDestination.ColumnName;
            GC.KeepAlive(columnName);
        }

        [TestMethod]
        public void Property_Reports_As_Set()
        {
            var destination = new MappingDestination();

            destination.ColumnName = "TestColumn";
            Assert.IsTrue(destination.IsPropertySet(MappingProperty.ColumnName));
            Assert.AreEqual(destination.ColumnName, "TestColumn");

            var random = new Random();

            int columnOrdinal = random.Next();
            destination.ColumnOrdinal = columnOrdinal;
            Assert.IsTrue(destination.IsPropertySet(MappingProperty.ColumnOrdinal));
            Assert.AreEqual(destination.ColumnOrdinal, columnOrdinal);

            int columnSize = random.Next();
            destination.ColumnSize = columnSize;
            Assert.IsTrue(destination.IsPropertySet(MappingProperty.ColumnSize));
            Assert.AreEqual(destination.ColumnSize, columnSize);

            destination.DataTypeName = "TestDataTypeName";
            Assert.IsTrue(destination.IsPropertySet(MappingProperty.DataTypeName));
            Assert.AreEqual(destination.DataTypeName, "TestDataTypeName");

            destination.IsKey = true;
            Assert.IsTrue(destination.IsPropertySet(MappingProperty.IsKey));
            Assert.AreEqual(destination.IsKey, true);
        }
    }
}
