namespace BulkWriter.Pipeline.Transforms
{
    public interface ITransformer<in TOut>
    {
        void Transform(TOut input);
    }
}
