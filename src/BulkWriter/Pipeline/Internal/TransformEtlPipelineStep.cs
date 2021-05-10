using System;
using System.Threading;
using System.Threading.Tasks;

namespace BulkWriter.Pipeline.Internal
{
    internal class TransformEtlPipelineStep<TOut> : EtlPipelineStep<TOut, TOut>
    {
        private readonly Action<TOut>[] _transformActions;

        public TransformEtlPipelineStep(EtlPipelineStepBase<TOut> previousStep, params Action<TOut>[] transformActions) : base(previousStep)
        {
            _transformActions = transformActions;
        }

        protected override Task RunCore(CancellationToken cancellationToken)
        {
            var enumerable = InputCollection.GetConsumingEnumerable(cancellationToken);

            foreach (var item in enumerable)
            {
                foreach (var transformAction in _transformActions)
                {
                    transformAction(item);
                }

                OutputCollection.Add(item, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
