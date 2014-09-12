using System;
using Headspring.BulkWriter.Properties;

namespace Headspring.BulkWriter
{
    internal class MapBuilderContextMap : IMapBuilderContextMap
    {
        private readonly PropertyMapping propertyMapping;

        public MapBuilderContextMap(PropertyMapping propertyMapping)
        {
            if (null == propertyMapping)
            {
                throw new ArgumentNullException("propertyMapping");
            }

            this.propertyMapping = propertyMapping;
        }

        public IMapBuilderContextMap ToColumnName(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException("name");
            }

            if (0 == name.Length)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToColumnName_InvalidColumName, "name");
            }

            this.propertyMapping.Destination.ColumnName = name;

            return this;
        }

        public IMapBuilderContextMap ToColumnOrdinal(int ordinal)
        {
            if (0 > ordinal)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToColumnOrdinal_InvalidOrdinal, "ordinal");
            }

            this.propertyMapping.Destination.ColumnOrdinal = ordinal;

            return this;
        }

        public IMapBuilderContextMap ToColumnSize(int size)
        {
            if (0 > size)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToColumnSize_InvalidColumnSize, "size");
            }

            this.propertyMapping.Destination.ColumnSize = size;

            return this;
        }

        public IMapBuilderContextMap ToDataTypeName(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException("name");
            }

            if (0 == name.Length)
            {
                throw new ArgumentException(Resources.MapBuilderContextMap_ToDataTypeName_InvalidName, "name");
            }

            this.propertyMapping.Destination.DataTypeName = name;

            return this;
        }

        public IMapBuilderContextMap AsKey()
        {
            this.propertyMapping.Destination.IsKey = true;

            return this;
        }

        public void DoNotMap()
        {
            this.propertyMapping.ShouldMap = false;
        }
    }
}