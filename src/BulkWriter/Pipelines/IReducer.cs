using System.Collections.Generic;

namespace BulkWriter.Pipelines
{
    public interface IReducer<in TInput, out TOutput>
    {
        IEnumerable<TOutput> Reduce(IEnumerable<TInput> input);
    }
}