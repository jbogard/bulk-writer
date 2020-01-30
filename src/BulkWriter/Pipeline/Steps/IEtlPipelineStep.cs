using System;
using System.Collections.Generic;
using BulkWriter.Pipeline.Transforms;

namespace BulkWriter.Pipeline.Steps
{
    public interface IEtlPipelineStep<out TIn, in TOut>
    {
        IEtlPipelineStep<TIn, TOut> Aggregate(IAggregator<TIn, TOut> aggregator);
        IEtlPipelineStep<TIn, TOut> Aggregate(Func<IEnumerable<TIn>, TOut> aggregationFunc);

        IEtlPipelineStep<TIn, TOut> Pivot(IPivot<TIn, TOut> pivot);
        IEtlPipelineStep<TIn, TOut> Pivot(Func<TIn, IEnumerable<TOut>> pivotFunc);

        IEtlPipelineStep<TIn, TOut> Project(IProjector<TIn, TOut> projector);
        IEtlPipelineStep<TIn, TOut> Project(Func<TIn, TOut> projectionFunc);

        IEtlPipeline WriteTo(IBulkWriter<TIn> bulkWriter);
    }
}