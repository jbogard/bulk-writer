using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class AggregateEtlPipelineStep<TIn, TOut> : EtlPipelineStep<TIn, TOut>
    {
        private readonly Func<IEnumerable<TIn>, TOut> _aggregationFunc;

        public AggregateEtlPipelineStep(EtlPipelineContext pipelineContext, BlockingCollection<TIn> inputCollection, Func<IEnumerable<TIn>, TOut> aggregationFunc) : base(pipelineContext, inputCollection)
        {
            _aggregationFunc = aggregationFunc ?? throw new ArgumentNullException(nameof(aggregationFunc));
        }

        public override void Run(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);

            RunSafely(() =>
            {
                var result = _aggregationFunc(enumerable);
                OutputCollection.Add(result, cancellationToken);
            });
        }
    }
}