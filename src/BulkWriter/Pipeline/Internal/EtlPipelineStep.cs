using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BulkWriter.Pipeline.Steps;
using BulkWriter.Pipeline.Transforms;

namespace BulkWriter.Pipeline.Internal
{
    internal abstract class EtlPipelineStep<TIn, TOut> : IEtlPipelineStep<TIn, TOut>, IEtlPipelineStep
    {
        protected readonly EtlPipelineContext PipelineContext;
        protected readonly BlockingCollection<TIn> InputCollection;
        protected readonly BlockingCollection<TOut> OutputCollection = new BlockingCollection<TOut>();

        protected EtlPipelineStep(EtlPipelineContext pipelineContext, BlockingCollection<TIn> inputCollection)
        {
            PipelineContext = pipelineContext;
            InputCollection = inputCollection;
        }

        public IEtlPipelineStep<TIn, TOut> Aggregate(IAggregator<TIn, TOut> aggregator)
        {
            if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));
            return Aggregate(aggregator.Aggregate);
        }

        public IEtlPipelineStep<TIn, TOut> Aggregate(Func<IEnumerable<TIn>, TOut> aggregationFunc)
        {
            if (aggregationFunc == null) throw new ArgumentNullException(nameof(aggregationFunc));

            var step = new AggregateEtlPipelineStep<TIn, TOut>(PipelineContext, InputCollection, aggregationFunc);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipelineStep<TIn, TOut> Pivot(IPivot<TIn, TOut> pivot)
        {
            if (pivot == null) throw new ArgumentNullException(nameof(pivot));
            return Pivot(pivot.Pivot);
        }

        public IEtlPipelineStep<TIn, TOut> Pivot(Func<TIn, IEnumerable<TOut>> pivotFunc)
        {
            if (pivotFunc == null) throw new ArgumentNullException(nameof(pivotFunc));

            var step = new PivotEtlPipelineStep<TIn, TOut>(PipelineContext, InputCollection, pivotFunc);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipelineStep<TIn, TOut> Project(IProjector<TIn, TOut> projector)
        {
            if (projector == null) throw new ArgumentNullException(nameof(projector));
            return Project(projector.ProjectTo);
        }

        public IEtlPipelineStep<TIn, TOut> Project(Func<TIn, TOut> projectionFunc)
        {
            if (projectionFunc == null) throw new ArgumentNullException(nameof(projectionFunc));

            var step = new ProjectEtlPipelineStep<TIn, TOut>(PipelineContext, InputCollection, projectionFunc);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipeline WriteTo(IBulkWriter<TIn> bulkWriter)
        {
            var step = new BulkWriterEtlPipelineStep<TIn>(PipelineContext, InputCollection, bulkWriter);
            PipelineContext.AddStep(step);

            return PipelineContext.Pipeline;
        }

        protected void RunSafely(Action action)
        {
            try
            {
                action();
            }
            finally
            {
                //This statement is in place to ensure that no matter what, the output collection
                //will be marked "complete".  Without this, an exception in the action above can
                //lead to a stalled (i.e. non-terminating) pipeline because this thread's consumer
                //is waiting for more output from this thread, which will never happen because the
                //thread is now dead.
                OutputCollection.CompleteAdding();
            }
        }

        public abstract void Run(CancellationToken cancellationToken);
    }
}