using System;

namespace BulkWriter.Internal
{
    public class PropertyMapping
    {
        public PropertyMapping(MappingSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ShouldMap = true;
        }

        public bool ShouldMap { get; set; }

        public MappingSource Source { get; }

        public MappingDestination Destination { get; } = new MappingDestination();
    }
}