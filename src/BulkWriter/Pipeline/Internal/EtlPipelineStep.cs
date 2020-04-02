using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BulkWriter.Pipeline.Steps;
using BulkWriter.Pipeline.Transforms;
using Microsoft.Extensions.Logging;

namespace BulkWriter.Pipeline.Internal
{
    internal abstract class EtlPipelineStepBase<TOut>
    {
        protected EtlPipelineStepBase(EtlPipelineContext pipelineContext, int stepNumber)
        {
            PipelineContext = pipelineContext;
            OutputCollection = new BlockingCollection<TOut>();
            StepNumber = stepNumber;
        }

        internal readonly EtlPipelineContext PipelineContext;
        internal readonly BlockingCollection<TOut> OutputCollection;

        public int StepNumber { get; protected set; }
    }

    internal abstract class EtlPipelineStep<TIn, TOut> : EtlPipelineStepBase<TOut>, IEtlPipelineStep<TIn, TOut>, IEtlPipelineStep
    {
        internal readonly BlockingCollection<TIn> InputCollection;

        protected EtlPipelineStep(EtlPipelineContext pipelineContext) : base(pipelineContext, 1)
        {
            InputCollection = new BlockingCollection<TIn>();
        }

        protected EtlPipelineStep(EtlPipelineStepBase<TIn> previousStep) : base(previousStep.PipelineContext, previousStep.StepNumber + 1)
        {
            InputCollection = previousStep.OutputCollection;
        }

        public IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(IAggregator<TOut, TNextOut> aggregator)
        {
            if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));
            return Aggregate(aggregator.Aggregate);
        }

        public IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(Func<IEnumerable<TOut>, TNextOut> aggregationFunc)
        {
            if (aggregationFunc == null) throw new ArgumentNullException(nameof(aggregationFunc));

            var step = new AggregateEtlPipelineStep<TOut, TNextOut>(this, aggregationFunc);
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

            var step = new PivotEtlPipelineStep<TOut, TNextOut>(this, pivotFunc);
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

            var step = new ProjectEtlPipelineStep<TOut, TNextOut>(this, projectionFunc);
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

            var step = new TransformEtlPipelineStep<TOut>(this, transformActions);
            PipelineContext.AddStep(step);

            return step;
        }

        public IEtlPipelineStep<TIn, TOut> LogWith(ILoggerFactory loggerFactory)
        {
            PipelineContext.LoggerFactory = loggerFactory;
            return this;
        }

        public IEtlPipeline WriteTo(IBulkWriter<TOut> bulkWriter)
        {
            var step = new BulkWriterEtlPipelineStep<TOut>(this, bulkWriter);
            PipelineContext.AddStep(step);

            return PipelineContext.Pipeline;
        }

        protected abstract void RunCore(CancellationToken cancellationToken);

        public void Run(CancellationToken cancellationToken)
        {
            try
            {
                var logger = PipelineContext.LoggerFactory?.CreateLogger(GetType());

                try
                {
                    logger?.LogInformation($"Starting pipeline step {StepNumber} of {PipelineContext.TotalSteps}");

                    RunCore(cancellationToken);

                    logger?.LogInformation($"Completing pipeline step {StepNumber} of {PipelineContext.TotalSteps}");
                }

                catch (Exception e)
                {
                    logger?.LogError(e, $"Error while running pipeline step {StepNumber} of {PipelineContext.TotalSteps}");
                    throw;
                }
            }

            finally
            {
                //This statement is in place to ensure that no matter what, the output collection
                //will be marked "complete". Without this, an exception in the try block above can
                //lead to a stalled (i.e. non-terminating) pipeline because this thread's consumer
                //is waiting for more output from this thread, which will never happen because the
                //thread is now dead. This should also ensure we get at least partial output in case
                //of an exception.
                OutputCollection.CompleteAdding();
            }
        }
    }
}