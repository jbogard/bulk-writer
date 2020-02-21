using System.Reflection;

namespace BulkWriter.Internal
{
    internal class MappingSource
    {
        public PropertyInfo Property { get; set; }

        public int Ordinal { get; set; }
    }
}