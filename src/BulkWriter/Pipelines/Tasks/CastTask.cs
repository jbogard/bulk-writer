using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace BulkWriter.Pipelines.Tasks
{
    internal class CastTask<TIn, TOut> : TaskBase<TOut>
    {
        private readonly BlockingCollection<TIn> _producingCollection;
        private readonly ICaster<TIn, TOut> _caster;
        private readonly IDecorator<TIn, TOut>[] _decorators;

        public CastTask(TaskFactory taskFactory, BlockingCollection<TIn> producingCollection, BlockingCollection<TOut> consumingCollection, ICaster<TIn, TOut> caster, params IDecorator<TIn, TOut>[] decorators) : base(taskFactory, consumingCollection)
        {
            _producingCollection = producingCollection;
            _caster = caster;
            _decorators = decorators;
        }

        public override void Run()
        {
            var enumerable = this._producingCollection.GetConsumingEnumerable(this.TaskFactory.CancellationToken);

            base.RunSafely(() =>
            {
                foreach (var item in enumerable)
                {
                    var result = _caster.Cast(item);

                    result = _decorators.Aggregate(result, (current, decorator) => decorator.Decorate(item, current));

                    this.ConsumingCollection.Add(result, this.TaskFactory.CancellationToken);
                }
            });
        }
    }
}