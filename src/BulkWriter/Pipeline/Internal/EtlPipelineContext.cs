using System;

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

        public void AddStep(IEtlPipelineStep step)
        {
            _addStepAction(Pipeline, step);
        }
    }
}