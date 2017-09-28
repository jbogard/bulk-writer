using System.Reflection;

namespace BulkWriter
{
    public class MappingSource
    {
        public PropertyInfo Property { get; set; }

        public int Ordinal { get; set; }
    }
}