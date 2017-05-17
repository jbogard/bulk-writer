namespace BulkWriter
{
    public interface IMapping<in TResult>
    {
        IBulkWriter<TResult> CreateBulkWriter(string connectionString);
    }
}