namespace BulkWriter.Connect
{
    public interface IConnectionStringProvider
    {
        string Get<T>(IPipelineContext<T> companyId);
    }
}