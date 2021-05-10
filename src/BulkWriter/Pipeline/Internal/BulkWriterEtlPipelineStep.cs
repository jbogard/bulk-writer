using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline.Internal
{
    internal class BulkWriterEtlPipelineStep<TEntity> : EtlPipelineStep<TEntity, TEntity>
    {
        private readonly IBulkWriter<TEntity> _bulkWriter;

        public BulkWriterEtlPipelineStep(EtlPipelineStepBase<TEntity> previousStep, IBulkWriter<TEntity> bulkWriter) : base(previousStep)
        {
            _bulkWriter = bulkWriter;
        }

        protected override async Task RunCore(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);
            await _bulkWriter.WriteToDatabaseAsync(enumerable);
        }
    }
}
