using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BulkWriter.Pipelines.Tasks
{
    internal abstract class TaskBase
    {
        protected readonly TaskFactory TaskFactory;

        protected TaskBase(TaskFactory taskFactory)
        {
            this.TaskFactory = taskFactory;
        }

        public abstract void Run();
    }

    internal abstract class TaskBase<T> : TaskBase
    {
        protected TaskBase(TaskFactory taskFactory, BlockingCollection<T> consumingCollection) : base(taskFactory)
        {
            ConsumingCollection = consumingCollection;
        }

        protected BlockingCollection<T> ConsumingCollection { get; }

        protected void RunSafely(Action action)
        {
            try
            {
                action();
            }
            finally
            {
                ConsumingCollection.CompleteAdding();
            }
        }
    }
}
