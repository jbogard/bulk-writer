namespace BulkWriter.Pipelines
{
    public interface IDecorator<in TInput, TOutput>
    {
        TOutput Decorate(TInput input, TOutput target);
    }
}