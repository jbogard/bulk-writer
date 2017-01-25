namespace BulkWriter
{
    public interface IMapBuilderContextMap
    {
        IMapBuilderContextMap ToColumnName(string name);

        IMapBuilderContextMap ToColumnOrdinal(int ordinal);

        IMapBuilderContextMap ToColumnSize(int size);

        IMapBuilderContextMap ToDataTypeName(string name);

        IMapBuilderContextMap AsKey();

        void DoNotMap();
    }
}