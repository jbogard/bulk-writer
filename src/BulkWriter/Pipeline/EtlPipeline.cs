using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BulkWriter.Pipeline.Internal;
using BulkWriter.Pipeline.Steps;

namespace BulkWriter.Pipeline
{
    /// <summary>
    /// Implements an ETL pipeline that ultimately writes to a BulkWriter object
    /// </summary>
    /// <inheritdoc cref="IEtlPipeline"/>
    public sealed class EtlPipeline : IEtlPipeline
    {
        private readonly Stack<IEtlPipelineStep> _pipelineSteps = new Stack<IEtlPipelineStep>();

        private EtlPipeline()
        {
        }

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

        /// <summary>
        /// Begins configuration of a new EtlPipeline
        /// </summary>
        /// <typeparam name="T">Type of input objects to the pipeline</typeparam>
        /// <param name="input">An enumerable with input objects for the pipeline</param>
        /// <returns>Object for continuation of pipeline configuration</returns>
        public static IEtlPipelineStep<T, T> StartWith<T>(IEnumerable<T> input)
        {
            var pipeline = new EtlPipeline();
            var etlPipelineSetupContext = new EtlPipelineContext(pipeline, (p, s) => pipeline._pipelineSteps.Push(s));
            var step = new StartEtlPipelineStep<T>(etlPipelineSetupContext, input);

            pipeline._pipelineSteps.Push(step);

            return step;
        }
    }
}
