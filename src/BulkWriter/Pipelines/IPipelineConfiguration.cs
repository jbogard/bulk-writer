using System.Collections.Concurrent;
using BulkWriter.Connect;

namespace BulkWriter.Pipelines
{
    public interface IPipelineConfiguration
    {
        IPipelineConfiguration StartWithCollection<T>(BlockingCollection<T> input, IGenerator<T> generator);
        IPipelineConfiguration Cast<TIn, TOut>(BlockingCollection<TIn> input, BlockingCollection<TOut> output, ICaster<TIn, TOut> caster, params IDecorator<TIn, TOut>[] decorators);
        IPipelineConfiguration Map<TIn, TOut>(BlockingCollection<TIn> input, BlockingCollection<TOut> output, IMapper<TIn, TOut> mapper);
        IPipelineConfiguration Reduce<TIn, TOut>(BlockingCollection<TIn> input, BlockingCollection<TOut> output, IReducer<TIn, TOut> reducer);
        IPipelineConfiguration Write<TEntity, T>(IPipelineContext<T> pipelineContext, BlockingCollection<TEntity> input);
    }
}
