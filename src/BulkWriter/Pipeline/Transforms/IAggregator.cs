using System.Collections.Generic;

namespace BulkWriter.Pipeline.Transforms
{
    public interface IAggregator<in TIn, out TOut>
    {
        TOut Aggregate(IEnumerable<TIn> input);
    }
}