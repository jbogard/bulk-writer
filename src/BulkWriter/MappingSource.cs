using System;
using System.Reflection;
using BulkWriter.Properties;

namespace BulkWriter
{
    public class MappingSource
    {
        public MappingSource(PropertyInfo property, int ordinal)
        {
            if (0 > ordinal)
            {
                throw new ArgumentException(Resources.MappingSource_InvalidOrdinal, nameof(ordinal));
            }

            Property = property ?? throw new ArgumentNullException(nameof(property));
            Ordinal = ordinal;
        }

        public PropertyInfo Property { get; }

        public int Ordinal { get; }
    }
}