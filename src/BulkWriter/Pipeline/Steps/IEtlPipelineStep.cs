using System;
using System.Collections.Generic;
using BulkWriter.Pipeline.Transforms;

namespace BulkWriter.Pipeline.Steps
{
    public interface IEtlPipelineStep<in TIn, TOut>
    {
        IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(IAggregator<TOut, TNextOut> aggregator);
        IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(Func<IEnumerable<TOut>, TNextOut> aggregationFunc);

        IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(IPivot<TOut, TNextOut> pivot);
        IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(Func<TOut, IEnumerable<TNextOut>> pivotFunc);

        IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(IProjector<TOut, TNextOut> projector);
        IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(Func<TOut, TNextOut> projectionFunc);

        IEtlPipeline WriteTo(IBulkWriter<TOut> bulkWriter);
    }
}