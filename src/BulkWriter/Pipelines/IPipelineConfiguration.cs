using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public interface ICaster<in TIn, out TOut>
    {
        TOut Cast(TIn input);
    }

    public interface IDecorator<in TInput, TOutput>
    {
        TOutput Decorate(TInput input, TOutput target);
    }

    public interface IGenerator<out T>
    {
        IEnumerable<T> Select();
    }

    public interface IMapper<in TInput, out TOutput>
    {
        IEnumerable<TOutput> Map(TInput input);
    }

    public interface IReducer<in TInput, out TOutput>
    {
        IEnumerable<TOutput> Reduce(IEnumerable<TInput> input);
    }
}
