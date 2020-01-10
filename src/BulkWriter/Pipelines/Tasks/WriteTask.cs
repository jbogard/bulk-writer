using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BulkWriter.Connect;

namespace BulkWriter.Pipelines.Tasks
{
    internal class WriteTask<TEntity, TContext> : TaskBase
    {
        private readonly IPipelineContext<TContext> _pipelineContext;
        private readonly IConnectionProvider _connectionProvider;
        private readonly BlockingCollection<TEntity> _inCollection;

        public WriteTask(TaskFactory taskFactory, IPipelineContext<TContext> pipelineContext, IConnectionProvider connectionProvider, BlockingCollection<TEntity> inCollection) : base(taskFactory)
        {
            _pipelineContext = pipelineContext;
            _connectionProvider = connectionProvider;
            _inCollection = inCollection;
        }

        public override void Run()
        {
            var enumerable = _inCollection.GetConsumingEnumerable(this.TaskFactory.CancellationToken);

            var connection = _connectionProvider.GetDestination();
            var connectionString = connection.Get(_pipelineContext);

            using (var bulkWriter = new BulkWriter<TEntity>(connectionString))
            {
                try
                {
                    bulkWriter.WriteToDatabase(enumerable);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}