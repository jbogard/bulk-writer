using System.Collections.Generic;
using System.Threading;

namespace BulkWriter.Pipeline.Internal
{
    internal class StartEtlPipelineStep<TIn> : EtlPipelineStep<TIn, TIn>
    {
        private readonly IEnumerable<TIn> _inputEnumerable;

        public StartEtlPipelineStep(EtlPipelineContext pipelineContext, IEnumerable<TIn> inputEnumerable) : base(pipelineContext)
        {
            _inputEnumerable = inputEnumerable;
        }

        protected override void RunCore(CancellationToken cancellationToken)
        {
            foreach (var item in _inputEnumerable)
            {
                OutputCollection.Add(item, cancellationToken);
            }
        }
    }
}