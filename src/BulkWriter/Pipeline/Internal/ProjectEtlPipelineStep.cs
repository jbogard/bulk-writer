using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class ProjectEtlPipelineStep<TIn, TOut> : EtlPipelineStep<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _projectionFunc;

        public ProjectEtlPipelineStep(EtlPipelineContext pipelineContext, BlockingCollection<TIn> inputCollection, Func<TIn, TOut> projectionFunc) : base(pipelineContext, inputCollection)
        {
            _projectionFunc = projectionFunc ?? throw new ArgumentNullException(nameof(projectionFunc));
        }

        public override void Run(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);

            RunSafely(() =>
            {
                foreach (var item in enumerable)
                {
                    var result = _projectionFunc(item);
                    OutputCollection.Add(result, cancellationToken);
                }
            });
        }
    }
}