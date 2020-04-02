using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class PivotEtlPipelineStep<TIn, TOut> : EtlPipelineStep<TIn, TOut>
    {
        private readonly Func<TIn, IEnumerable<TOut>> _pivotFunc;

        public PivotEtlPipelineStep(EtlPipelineContext pipelineContext, BlockingCollection<TIn> inputCollection, Func<TIn, IEnumerable<TOut>> pivotFunc) : base(pipelineContext, inputCollection)
        {
            _pivotFunc = pivotFunc ?? throw new ArgumentNullException(nameof(pivotFunc));
        }

        protected override void RunCore(CancellationToken cancellationToken)
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
        }
    }
}