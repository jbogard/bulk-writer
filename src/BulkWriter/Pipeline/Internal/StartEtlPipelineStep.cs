using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class StartEtlPipelineStep<TIn> : EtlPipelineStep<TIn, TIn>
    {
        private readonly IEnumerable<TIn> _inputEnumerable;

        public StartEtlPipelineStep(EtlPipelineContext pipelineContext, IEnumerable<TIn> inputEnumerable) : base(pipelineContext, new BlockingCollection<TIn>())
        {
            _inputEnumerable = inputEnumerable;
        }

        public override void Run(CancellationToken cancellationToken)
        {
            RunSafely(() =>
            {
                foreach (var item in _inputEnumerable)
                {
                    OutputCollection.Add(item, cancellationToken);
                }
            });
        }
    }
}