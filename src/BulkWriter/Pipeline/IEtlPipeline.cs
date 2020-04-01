using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline
{
    public interface IEtlPipeline
    {
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
    }
}