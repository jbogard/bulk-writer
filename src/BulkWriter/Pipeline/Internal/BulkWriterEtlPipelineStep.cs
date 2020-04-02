using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class BulkWriterEtlPipelineStep<TEntity> : EtlPipelineStep<TEntity, TEntity>
    {
        private readonly IBulkWriter<TEntity> _bulkWriter;

        public BulkWriterEtlPipelineStep(EtlPipelineStepBase<TEntity> previousStep, IBulkWriter<TEntity> bulkWriter) : base(previousStep)
        {
            _bulkWriter = bulkWriter;
        }

        protected override void RunCore(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);
            _bulkWriter.WriteToDatabase(enumerable);
        }
    }
}