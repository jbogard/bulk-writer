using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BulkWriter.Properties;

namespace BulkWriter
{
    internal class MappingDestination
    {
        internal const int PropertyIndexCount = 5;

        private readonly BitArray propertiesSet = new BitArray(PropertyIndexCount);

        private string columnName;
        private int columnSize;
        private string dataType;
        private bool isKey;
        private int ordinal;

        public string ColumnName
        {
            get { return this.GetPropertyValue(MappingProperty.ColumnName, ref this.columnName); }
            internal set { this.SetPropertyValue(MappingProperty.ColumnName, value, ref this.columnName); }
        }

        public int ColumnOrdinal
        {
            get { return this.GetPropertyValue(MappingProperty.ColumnOrdinal, ref this.ordinal); }
            internal set { this.SetPropertyValue(MappingProperty.ColumnOrdinal, value, ref this.ordinal); }
        }

        public int ColumnSize
        {
            get { return this.GetPropertyValue(MappingProperty.ColumnSize, ref this.columnSize); }
            internal set { this.SetPropertyValue(MappingProperty.ColumnSize, value, ref this.columnSize); }
        }

        public string DataTypeName
        {
            get { return this.GetPropertyValue(MappingProperty.DataTypeName, ref this.dataType); }
            internal set { this.SetPropertyValue(MappingProperty.DataTypeName, value, ref this.dataType); }
        }

        public bool IsKey
        {
            get { return this.GetPropertyValue(MappingProperty.IsKey, ref this.isKey); }
            internal set { this.SetPropertyValue(MappingProperty.IsKey, value, ref this.isKey); }
        }

        public bool IsPropertySet(MappingProperty property)
        {
            return this.IsPropertySet((int) property);
        }

        public bool IsPropertySet(int propertyIndex)
        {
            return this.propertiesSet[propertyIndex];
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IsPropertySet")]
        private T GetPropertyValue<T>(MappingProperty property, ref T field)
        {
            if (!this.propertiesSet[(int) property])
            {
                throw new InvalidOperationException(string.Format(Resources.Culture, Resources.MappingDestination_GetPropertyValue_PropertyNotSet, property));
            }

            return field;
        }

        // ReSharper disable once RedundantAssignment
        private void SetPropertyValue<T>(MappingProperty property, T value, ref T field)
        {
            field = value;
            this.propertiesSet[(int) property] = true;
        }
    }
}