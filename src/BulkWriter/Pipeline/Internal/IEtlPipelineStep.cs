using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline.Internal
{
    internal interface IEtlPipelineStep
    {
        Task Run(CancellationToken cancellationToken);
    }
}
