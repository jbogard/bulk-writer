using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class StartEtlPipelineStep<TIn> : EtlPipelineStep<TIn, TIn>
    {
        public StartEtlPipelineStep(EtlPipelineContext pipelineContext, IEnumerable<TIn> enumerable) : base(pipelineContext, new BlockingCollection<TIn>(new ConcurrentQueue<TIn>(enumerable)))
        {
        }

        public override void Run(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);

            RunSafely(() =>
            {
                foreach (var item in enumerable)
                {
                    OutputCollection.Add(item, cancellationToken);
                }
            });
        }
    }
}