using System;
using Microsoft.Extensions.Logging;

namespace BulkWriter.Pipeline.Internal
{
    internal class EtlPipelineContext
    {
        private readonly Action<IEtlPipeline, IEtlPipelineStep> _addStepAction;

        public EtlPipelineContext(IEtlPipeline etlPipeline, Action<IEtlPipeline, IEtlPipelineStep> addStepAction)
        {
            Pipeline = etlPipeline;
            _addStepAction = addStepAction;
        }

        public IEtlPipeline Pipeline { get; }
        public int StepCount { get; private set; } = 0;
        public ILogger Logger { get; set; }

        public void AddStep(IEtlPipelineStep step)
        {
            ++StepCount;
            _addStepAction(Pipeline, step);
        }
    }
}