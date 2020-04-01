using System.Threading;
using Microsoft.Extensions.Logging;

namespace BulkWriter.Pipeline.Internal
{
    internal interface IEtlPipelineStep
    {
        ILogger Logger { get; set; }
        void Run(CancellationToken cancellationToken);
    }
}