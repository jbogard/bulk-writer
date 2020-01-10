using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BulkWriter.Pipelines.Tasks
{
    internal class ReduceTask<TIn, TOut> : TaskBase<TOut>
    {
        private readonly BlockingCollection<TIn> _producingCollection;
        private readonly IReducer<TIn, TOut> _reducer;

        public ReduceTask(TaskFactory taskFactory, BlockingCollection<TIn> producingCollection, BlockingCollection<TOut> consumingCollection, IReducer<TIn, TOut> reducer) : base(taskFactory, consumingCollection)
        {
            _producingCollection = producingCollection;
            _reducer = reducer;
        }

        public override void Run()
        {
            var enumerable = _producingCollection.GetConsumingEnumerable(this.TaskFactory.CancellationToken);

            base.RunSafely(() =>
            {
                var outputs = _reducer.Reduce(enumerable);
                foreach (var output in outputs)
                {
                    this.ConsumingCollection.Add(output, this.TaskFactory.CancellationToken);
                }
            });
        }
    }
}