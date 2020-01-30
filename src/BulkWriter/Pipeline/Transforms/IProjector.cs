namespace BulkWriter.Pipeline.Transforms
{
    public interface IProjector<in TIn, out TOut>
    {
        TOut ProjectTo(TIn input);
    }
}