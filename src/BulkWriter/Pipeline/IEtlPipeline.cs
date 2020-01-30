using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline
{
    public interface IEtlPipeline
    {
        Task ExecuteAsync();
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}