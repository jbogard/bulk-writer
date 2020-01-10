using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BulkWriter.Pipelines.Tasks
{
    internal class GeneratorTask<T> : TaskBase<T>
    {
        private readonly IGenerator<T> _generator;

        public GeneratorTask(TaskFactory taskFactory, BlockingCollection<T> consumingCollection, IGenerator<T> generator) : base(taskFactory, consumingCollection)
        {
            _generator = generator;
        }

        public override void Run()
        {
            this.RunSafely(() =>
            {
                foreach (var item in this._generator.Select())
                {
                    this.ConsumingCollection.Add(item, this.TaskFactory.CancellationToken);
                }
            });
        }
    }
}