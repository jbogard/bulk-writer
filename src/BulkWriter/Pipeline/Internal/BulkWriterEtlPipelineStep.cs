using System.Collections.Concurrent;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class BulkWriterEtlPipelineStep<TEntity> : EtlPipelineStep<TEntity, TEntity>
    {
        private readonly IBulkWriter<TEntity> _bulkWriter;

        public BulkWriterEtlPipelineStep(EtlPipelineContext pipelineContext, BlockingCollection<TEntity> inputCollection, IBulkWriter<TEntity> bulkWriter) : base(pipelineContext, inputCollection)
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