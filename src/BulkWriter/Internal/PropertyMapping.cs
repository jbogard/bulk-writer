namespace BulkWriter.Internal
{
    internal class PropertyMapping
    {
        public bool ShouldMap { get; set; }

        public MappingSource Source { get; set; }

        public MappingDestination Destination { get; set; }
    }
}