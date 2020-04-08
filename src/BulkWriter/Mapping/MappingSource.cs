using System.Reflection;

namespace BulkWriter.Mapping
{
    /// <summary>
    /// Defines a source property for mapping via an <see cref="EnumerableDataReader{TResult}"/>
    /// </summary>
    public class MappingSource
    {
        /// <summary>
        /// The property to be mapped
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// The order in which this property should be mapped
        /// </summary>
        public int Ordinal { get; set; }
    }
}