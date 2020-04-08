namespace BulkWriter.Mapping
{
    /// <summary>
    /// Defines a manual mapping for a given property on a type. Used by the <see cref="EnumerableDataReader{TResult}"/> class
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// True if a property should be mapped in the <see cref="EnumerableDataReader{TResult}"/>, false otherwise
        /// </summary>
        public bool ShouldMap { get; set; }

        /// <summary>
        /// Configuration identifying a source property to map
        /// </summary>
        public MappingSource Source { get; set; }

        /// <summary>
        /// Configuration for how the <see cref="MappingSource"/> property should be mapped in the output of the <see cref="EnumerableDataReader{TResult}"/>
        /// </summary>
        public MappingDestination Destination { get; set; }
    }
}