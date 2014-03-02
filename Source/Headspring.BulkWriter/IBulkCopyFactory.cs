namespace Headspring.BulkWriter
{
    public interface IBulkCopyFactory
    {
        IBulkCopy Create(object item, out IPropertyToOrdinalMappings mappings);
    }
}