using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BulkWriter.Pipeline
{
    public interface IEtlPipeline
    {
        /// <summary>
        /// Number of steps in the pipeline
        /// </summary>
        int StepCount { get; }

        /// <summary>
        /// Executes the previously configured pipeline
        /// </summary>
        /// <returns>Awaitable Task for the running pipeline</returns>
        Task ExecuteAsync();

        /// <summary>
        /// Executes the previously configured pipeline in a cancellable fashion
        /// </summary>
        /// <param name="cancellationToken">Token for cancelling the pipeline mid-run</param>
        /// <returns>Awaitable Task for the running pipeline</returns>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Enable logging for steps in this pipeline
        /// </summary>
        /// <param name="logger">Logger object to which messages will be sent</param>
        /// <returns>Current ETL Pipeline</returns>
        IEtlPipeline LogTo(ILogger logger);
    }
}