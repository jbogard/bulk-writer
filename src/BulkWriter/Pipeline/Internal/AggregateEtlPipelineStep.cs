using System;
using System.Collections.Generic;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class AggregateEtlPipelineStep<TIn, TOut> : EtlPipelineStep<TIn, TOut>
    {
        private readonly Func<IEnumerable<TIn>, TOut> _aggregationFunc;

        public AggregateEtlPipelineStep(EtlPipelineStepBase<TIn> previousStep, Func<IEnumerable<TIn>, TOut> aggregationFunc) : base(previousStep)
        {
            _aggregationFunc = aggregationFunc ?? throw new ArgumentNullException(nameof(aggregationFunc));
        }

        protected override void RunCore(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);

            var result = _aggregationFunc(enumerable);
            OutputCollection.Add(result, cancellationToken);
        }
    }
}