using System.Collections.Generic;

namespace BulkWriter.Pipeline.Transforms
{
    public interface IAggregator<in TIn, out TOut>
    {
        /// <summary>
        /// Aggregates multiple input objects down to a single output
        /// </summary>
        /// <param name="input">Set of input objects to aggregate</param>
        /// <returns>Aggregated result object</returns>
        TOut Aggregate(IEnumerable<TIn> input);
    }
}