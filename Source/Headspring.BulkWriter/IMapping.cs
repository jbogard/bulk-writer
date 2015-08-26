namespace Headspring.BulkWriter
{
    public interface IMapping<TResult>
    {
        IBulkWriter<TResult> CreateBulkWriter(string connectionString);

        IBulkWriter<TResult> CreateBulkWriter(BulkWriterOptions options);
    }
}