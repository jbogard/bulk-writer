namespace BulkWriter.Pipeline.Transforms
{
    public interface IProjector<in TIn, out TOut>
    {
        /// <summary>
        /// Projects the input object to a new object type
        /// </summary>
        /// <param name="input">Input object to be projected</param>
        /// <returns>Projected object</returns>
        TOut ProjectTo(TIn input);
    }
}