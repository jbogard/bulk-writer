using System;
using System.Reflection;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class MappingSource
    {
        private readonly PropertyInfo property;
        private readonly int ordinal;

        public MappingSource(PropertyInfo property, int ordinal)
        {
            if (null == property)
            {
                throw new ArgumentNullException("property");
            }

            if (0 > ordinal)
            {
                throw new ArgumentException(Resources.MappingSource_InvalidOrdinal, "ordinal");
            }

            this.property = property;
            this.ordinal = ordinal;
        }

        public PropertyInfo Property
        {
            get { return this.property; }
        }

        public int Ordinal
        {
            get { return this.ordinal; }
        }
    }
}