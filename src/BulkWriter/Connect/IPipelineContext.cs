namespace BulkWriter.Connect
{
    public interface IPipelineContext<out T>
    {
        T Context { get; }
    }
}
