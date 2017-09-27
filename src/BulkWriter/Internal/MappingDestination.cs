using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class MappingDestination
    {
        public const int PropertyIndexCount = 5;

        private readonly BitArray _propertiesSet = new BitArray(PropertyIndexCount);

        private string _columnName;
        private int _columnSize;
        private string _dataType;
        private bool _isKey;
        private int _ordinal;

        public string ColumnName
        {
            get => GetPropertyValue(MappingProperty.ColumnName, ref _columnName);
            set => SetPropertyValue(MappingProperty.ColumnName, value, ref _columnName);
        }

        public int ColumnOrdinal
        {
            get => GetPropertyValue(MappingProperty.ColumnOrdinal, ref _ordinal);
            set => SetPropertyValue(MappingProperty.ColumnOrdinal, value, ref _ordinal);
        }

        public int ColumnSize
        {
            get => GetPropertyValue(MappingProperty.ColumnSize, ref _columnSize);
            set => SetPropertyValue(MappingProperty.ColumnSize, value, ref _columnSize);
        }

        public string DataTypeName
        {
            get => GetPropertyValue(MappingProperty.DataTypeName, ref _dataType);
            set => SetPropertyValue(MappingProperty.DataTypeName, value, ref _dataType);
        }

        public bool IsKey
        {
            get => GetPropertyValue(MappingProperty.IsKey, ref _isKey);
            set => SetPropertyValue(MappingProperty.IsKey, value, ref _isKey);
        }

        public bool IsPropertySet(MappingProperty property) => IsPropertySet((int) property);

        public bool IsPropertySet(int propertyIndex) => _propertiesSet[propertyIndex];

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IsPropertySet")]
        private T GetPropertyValue<T>(MappingProperty property, ref T field)
        {
            if (!_propertiesSet[(int) property])
            {
                throw new InvalidOperationException(string.Format(Resources.Culture, Resources.MappingDestination_GetPropertyValue_PropertyNotSet, property));
            }

            return field;
        }

        // ReSharper disable once RedundantAssignment
        private void SetPropertyValue<T>(MappingProperty property, T value, ref T field)
        {
            field = value;
            _propertiesSet[(int) property] = true;
        }
    }
}