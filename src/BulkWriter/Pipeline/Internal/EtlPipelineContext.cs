using System;
using Microsoft.Extensions.Logging;

namespace BulkWriter.Pipeline.Internal
{
    internal class EtlPipelineContext
    {
        private readonly Action<IEtlPipelineStep> _addStepAction;

        public EtlPipelineContext(IEtlPipeline etlPipeline, Action<IEtlPipelineStep> addStepAction)
        {
            Pipeline = etlPipeline;
            _addStepAction = addStepAction;
        }

        public IEtlPipeline Pipeline { get; }
        public ILoggerFactory LoggerFactory { get; set; }

        public void AddStep(IEtlPipelineStep step)
        {
            _addStepAction(step);
        }
    }
}