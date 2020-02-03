using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(IAggregator<TOut, TNextOut> aggregator)
        {
            if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));
            return Aggregate(aggregator.Aggregate);
        }

        public IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(Func<IEnumerable<TOut>, TNextOut> aggregationFunc)
        {
            if (aggregationFunc == null) throw new ArgumentNullException(nameof(aggregationFunc));

            var step = new AggregateEtlPipelineStep<TOut, TNextOut>(PipelineContext, OutputCollection, aggregationFunc);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(IPivot<TOut, TNextOut> pivot)
        {
            if (pivot == null) throw new ArgumentNullException(nameof(pivot));
            return Pivot(pivot.Pivot);
        }

        public IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(Func<TOut, IEnumerable<TNextOut>> pivotFunc)
        {
            if (pivotFunc == null) throw new ArgumentNullException(nameof(pivotFunc));

            var step = new PivotEtlPipelineStep<TOut, TNextOut>(PipelineContext, OutputCollection, pivotFunc);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(IProjector<TOut, TNextOut> projector)
        {
            if (projector == null) throw new ArgumentNullException(nameof(projector));
            return Project(projector.ProjectTo);
        }

        public IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(Func<TOut, TNextOut> projectionFunc)
        {
            if (projectionFunc == null) throw new ArgumentNullException(nameof(projectionFunc));

            var step = new ProjectEtlPipelineStep<TOut, TNextOut>(PipelineContext, OutputCollection, projectionFunc);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipelineStep<TOut, TOut> TransformInPlace(params ITransformer<TOut>[] transformers)
        {
            if (transformers == null || transformers.Any(t => t == null)) throw new ArgumentNullException(nameof(transformers), @"No transformer may be null");
            return TransformInPlace(transformers.Select(t => (Action<TOut>)t.Transform).ToArray());
        }

        public IEtlPipelineStep<TOut, TOut> TransformInPlace(params Action<TOut>[] transformActions)
        {
            if (transformActions == null || transformActions.Any(t => t == null)) throw new ArgumentNullException(nameof(transformActions), @"No transformer may be null");

            var step = new TransformEtlPipelineStep<TOut>(PipelineContext, OutputCollection, transformActions);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipeline WriteTo(IBulkWriter<TOut> bulkWriter)
        {
            var step = new BulkWriterEtlPipelineStep<TOut>(PipelineContext, OutputCollection, bulkWriter);
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