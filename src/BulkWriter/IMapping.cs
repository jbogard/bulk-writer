namespace BulkWriter
{
    public interface IMapping<TResult>
    {
        IBulkWriter<TResult> CreateBulkWriter(string connectionString);
    }
}