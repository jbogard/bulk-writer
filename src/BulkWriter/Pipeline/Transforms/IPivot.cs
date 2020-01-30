using System.Collections.Generic;

namespace BulkWriter.Pipeline.Transforms
{
    public interface IPivot<in TIn, out TOut>
    {
        IEnumerable<TOut> Pivot(TIn input);
    }
}