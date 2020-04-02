using System;

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

        public void AddStep(IEtlPipelineStep step)
        {
            _addStepAction(step);
        }
    }
}