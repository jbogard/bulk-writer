using System.Collections.Generic;
using System.Linq;
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
        private readonly List<IEtlPipelineStep> _pipelineSteps = new List<IEtlPipelineStep>();

        private EtlPipeline()
        {
        }

        public Task ExecuteAsync()
        {
            return ExecuteAsync(CancellationToken.None);
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var pipelineTasks = _pipelineSteps.Select(s => Task.Run(() => s.Run(cancellationToken), cancellationToken));
            return Task.WhenAll(pipelineTasks);
        }

        private void AddStep(IEtlPipelineStep etlPipelineStep)
        {
            _pipelineSteps.Add(etlPipelineStep);
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
            var etlPipelineSetupContext = new EtlPipelineContext(pipeline, s => pipeline.AddStep(s));
            var step = new StartEtlPipelineStep<T>(etlPipelineSetupContext, input);

            etlPipelineSetupContext.AddStep(step);

            return step;
        }
    }
}
