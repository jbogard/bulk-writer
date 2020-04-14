namespace BulkWriter.Pipeline.Transforms
{
    public interface ITransformer<in TOut>
    {
        /// <summary>
        /// Transforms an input object in-place (i.e. via side-effects)
        /// </summary>
        /// <param name="input">Input object modified by the transform</param>
        void Transform(TOut input);
    }
}
