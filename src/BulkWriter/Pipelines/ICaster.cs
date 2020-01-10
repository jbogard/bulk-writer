namespace BulkWriter.Pipelines
{
    public interface ICaster<in TIn, out TOut>
    {
        TOut Cast(TIn input);
    }
}