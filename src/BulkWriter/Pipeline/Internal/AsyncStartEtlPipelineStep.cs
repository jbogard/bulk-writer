#if NETSTANDARD2_1
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline.Internal
{
    internal class AsyncStartEtlPipelineStep<TIn> : EtlPipelineStep<TIn, TIn>
    {
        private readonly IAsyncEnumerable<TIn> _inputEnumerable;

        public AsyncStartEtlPipelineStep(EtlPipelineContext pipelineContext, IAsyncEnumerable<TIn> inputEnumerable) : base(pipelineContext)
        {
            _inputEnumerable = inputEnumerable;
        }

        protected override async Task RunCore(CancellationToken cancellationToken)
        {
            await foreach (var item in _inputEnumerable.WithCancellation(cancellationToken))
            {
                OutputCollection.Add(item, cancellationToken);
            }
        }
    }
}
#endif
