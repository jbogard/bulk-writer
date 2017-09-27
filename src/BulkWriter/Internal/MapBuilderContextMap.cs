using System;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class MapBuilderContextMap : IMapBuilderContextMap
    {
        private readonly PropertyMapping _propertyMapping;

        public MapBuilderContextMap(PropertyMapping propertyMapping) => _propertyMapping = propertyMapping ?? throw new ArgumentNullException(nameof(propertyMapping));

        public IMapBuilderContextMap ToColumnName(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (0 == name.Length)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToColumnName_InvalidColumName, nameof(name));
            }

            _propertyMapping.Destination.ColumnName = name;

            return this;
        }

        public IMapBuilderContextMap ToColumnOrdinal(int ordinal)
        {
            if (0 > ordinal)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToColumnOrdinal_InvalidOrdinal, nameof(ordinal));
            }

            _propertyMapping.Destination.ColumnOrdinal = ordinal;

            return this;
        }

        public IMapBuilderContextMap ToColumnSize(int size)
        {
            if (0 > size)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToColumnSize_InvalidColumnSize, nameof(size));
            }

            _propertyMapping.Destination.ColumnSize = size;

            return this;
        }

        public IMapBuilderContextMap ToDataTypeName(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (0 == name.Length)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToDataTypeName_InvalidName, nameof(name));
            }

            _propertyMapping.Destination.DataTypeName = name;

            return this;
        }

        public IMapBuilderContextMap AsKey()
        {
            _propertyMapping.Destination.IsKey = true;

            return this;
        }

        public void DoNotMap()
        {
            _propertyMapping.ShouldMap = false;
        }
    }
}