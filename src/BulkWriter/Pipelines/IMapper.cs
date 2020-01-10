using System.Collections.Generic;

namespace BulkWriter.Pipelines
{
    public interface IMapper<in TInput, out TOutput>
    {
        IEnumerable<TOutput> Map(TInput input);
    }
}