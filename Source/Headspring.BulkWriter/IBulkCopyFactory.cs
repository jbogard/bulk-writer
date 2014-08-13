namespace Headspring.BulkWriter
{
    public interface IBulkCopyFactory
    {
        IBulkCopy Create(object item, BulkWriterOptions bulkWriterOptions, out IPropertyToOrdinalMappings mappings);
    }
}