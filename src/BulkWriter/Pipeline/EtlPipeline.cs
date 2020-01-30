using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BulkWriter.Pipeline.Internal;
using BulkWriter.Pipeline.Steps;

namespace BulkWriter.Pipeline
{
    public class EtlPipeline : IEtlPipeline
    {
        private readonly Stack<IEtlPipelineStep> _pipelineSteps = new Stack<IEtlPipelineStep>();

        public Task ExecuteAsync()
        {
            return ExecuteAsync(CancellationToken.None);
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var finalStep = _pipelineSteps.Pop();
            var finalTask = Task.Run(() => finalStep.Run(cancellationToken), cancellationToken);

            while (_pipelineSteps.Count != 0)
            {
                var taskAction = _pipelineSteps.Pop();
                Task.Run(() => taskAction.Run(cancellationToken), cancellationToken);
            }

            return finalTask;
        }

        public IEtlPipelineStep<T, T> StartWith<T>(IEnumerable<T> input)
        {
            var etlPipelineSetupContext = new EtlPipelineContext(this, (p, s) => _pipelineSteps.Push(s));
            var step = new StartEtlPipelineStep<T>(etlPipelineSetupContext, input);

            _pipelineSteps.Push(step);

            return step;
        }
    }
}
