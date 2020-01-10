using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BulkWriter.Pipelines.Tasks
{
    internal class MapTask<TIn, TOut> : TaskBase<TOut>
    {
        private readonly BlockingCollection<TIn> _producingCollection;
        private readonly IMapper<TIn, TOut> _mapper;

        public MapTask(TaskFactory taskFactory, BlockingCollection<TIn> producingCollection, BlockingCollection<TOut> consumingCollection, IMapper<TIn, TOut> mapper) : base(taskFactory, consumingCollection)
        {
            _producingCollection = producingCollection;
            _mapper = mapper;
        }

        public override void Run()
        {
            var enumerable = _producingCollection.GetConsumingEnumerable(this.TaskFactory.CancellationToken);

            this.RunSafely(() =>
            {
                foreach (var item in enumerable)
                {
                    var outputs = _mapper.Map(item);
                    foreach (var output in outputs)
                    {
                        this.ConsumingCollection.Add(output, this.TaskFactory.CancellationToken);
                    }
                }
            });
        }
    }
}