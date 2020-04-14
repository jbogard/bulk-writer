using System.Collections.Generic;

namespace BulkWriter.Pipeline.Transforms
{
    public interface IPivot<in TIn, out TOut>
    {
        /// <summary>
        /// Pivots a single input object out to multiple output objects
        /// </summary>
        /// <param name="input">Input object to pivot</param>
        /// <returns>Enumerable with zero or more output objects</returns>
        IEnumerable<TOut> Pivot(TIn input);
    }
}