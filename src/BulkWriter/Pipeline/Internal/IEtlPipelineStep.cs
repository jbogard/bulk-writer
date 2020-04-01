using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal interface IEtlPipelineStep
    {
        void Run(CancellationToken cancellationToken);
    }
}