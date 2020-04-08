using System.Reflection;

namespace BulkWriter.Mapping
{
    internal class MappingSource
    {
        public PropertyInfo Property { get; set; }

        public int Ordinal { get; set; }
    }
}