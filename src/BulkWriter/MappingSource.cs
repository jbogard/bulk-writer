using System.Reflection;

namespace BulkWriter
{
    internal class MappingSource
    {
        public PropertyInfo Property { get; set; }

        public int Ordinal { get; set; }
    }
}