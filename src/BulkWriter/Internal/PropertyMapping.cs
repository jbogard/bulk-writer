using System;

namespace BulkWriter.Internal
{
    public class PropertyMapping
    {
        private readonly MappingDestination destination = new MappingDestination();
        private readonly MappingSource source;

        public PropertyMapping(MappingSource source)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            this.source = source;
            this.ShouldMap = true;
        }

        public bool ShouldMap { get; set; }

        public MappingSource Source
        {
            get { return this.source; }
        }

        public MappingDestination Destination
        {
            get { return this.destination; }
        }
    }
}