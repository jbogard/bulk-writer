namespace BulkWriter.Connect
{
    public interface IConnectionProvider
    {
        IConnectionStringProvider GetSource();

        IConnectionStringProvider GetDestination();
    }
}
