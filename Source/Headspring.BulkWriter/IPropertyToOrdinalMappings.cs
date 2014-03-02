namespace Headspring.BulkWriter
{
    public interface IPropertyToOrdinalMappings
    {
        int FieldCount { get; }

        int GetOrdinal(string propertyName);

        object GetValue(int ordinal, object item);

        string GetName(int ordinal);
    }
}