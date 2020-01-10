using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BulkWriter.Connect;
using BulkWriter.Pipelines.Tasks;

namespace BulkWriter.Pipelines
{
    public abstract class Pipeline
    {
        private readonly TaskFactory taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
        private readonly Stack<TaskBase> taskStack = new Stack<TaskBase>();
        private readonly PipelineConfiguration pipelineConfiguration;

        protected Pipeline(IConnectionProvider connectionProvider)
        {
            this.pipelineConfiguration = new PipelineConfiguration(connectionProvider, this.taskFactory, this.taskStack);
        }

        protected void SetUpPipeline(Action<IPipelineConfiguration> action)
        {
            action(this.pipelineConfiguration);
        }

        public virtual Task RunAsync(CancellationToken cancellationToken)
        {
            var writeAction = this.taskStack.Pop();
            var writeTask = Task.Run(() => writeAction.Run(), cancellationToken);

            while (this.taskStack.Count != 0)
            {
                var taskAction = this.taskStack.Pop();

                Task.Run(() => taskAction.Run(), cancellationToken);
            }

            return writeTask;
        }

        private class PipelineConfiguration : IPipelineConfiguration
        {
            private readonly IConnectionProvider connectionProvider;
            private readonly TaskFactory taskFactory;
            private readonly Stack<TaskBase> taskBuilders;

            public PipelineConfiguration(IConnectionProvider connectionProvider, TaskFactory taskFactory, Stack<TaskBase> taskBuilders)
            {
                this.connectionProvider = connectionProvider;
                this.taskFactory = taskFactory;
                this.taskBuilders = taskBuilders;
            }

            public IPipelineConfiguration StartWithCollection<T>(BlockingCollection<T> input, IGenerator<T> generator)
            {
                if (this.taskBuilders.Any())
                {
                    throw new InvalidOperationException($"{nameof(this.StartWithCollection)} must be called before any other configuration methods.");
                }

                this.taskBuilders.Push(new GeneratorTask<T>(this.taskFactory, input, generator));
                return this;
            }

            public IPipelineConfiguration Cast<TIn, TOut>(BlockingCollection<TIn> input, BlockingCollection<TOut> output, ICaster<TIn, TOut> caster, params IDecorator<TIn, TOut>[] decorators)
            {
                this.taskBuilders.Push(new CastTask<TIn, TOut>(this.taskFactory, input, output, caster, decorators));
                return this;
            }

            public IPipelineConfiguration Map<TIn, TOut>(BlockingCollection<TIn> input, BlockingCollection<TOut> output, IMapper<TIn, TOut> mapper)
            {
                this.taskBuilders.Push(new MapTask<TIn, TOut>(this.taskFactory, input, output, mapper));
                return this;
            }

            public IPipelineConfiguration Reduce<TIn, TOut>(BlockingCollection<TIn> input, BlockingCollection<TOut> output, IReducer<TIn, TOut> reducer)
            {
                this.taskBuilders.Push(new ReduceTask<TIn, TOut>(this.taskFactory, input, output, reducer));
                return this;
            }

            public IPipelineConfiguration Write<TEntity, T>(IPipelineContext<T> pipelineContext, BlockingCollection<TEntity> input)
            {
                this.taskBuilders.Push(new WriteTask<TEntity, T>(this.taskFactory, pipelineContext, connectionProvider, input));
                return this;
            }
        }
    }
}
