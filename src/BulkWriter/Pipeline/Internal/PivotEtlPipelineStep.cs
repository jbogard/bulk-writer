using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline.Internal
{
    internal class PivotEtlPipelineStep<TIn, TOut> : EtlPipelineStep<TIn, TOut>
    {
        private readonly Func<TIn, IEnumerable<TOut>> _pivotFunc;

        public PivotEtlPipelineStep(EtlPipelineStepBase<TIn> previousStep, Func<TIn, IEnumerable<TOut>> pivotFunc) : base(previousStep)
        {
            _pivotFunc = pivotFunc ?? throw new ArgumentNullException(nameof(pivotFunc));
        }

        protected override Task RunCore(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);

            foreach (var item in enumerable)
            {
                var outputs = _pivotFunc(item);
                foreach (var output in outputs)
                {
                    OutputCollection.Add(output, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }
    }
}
