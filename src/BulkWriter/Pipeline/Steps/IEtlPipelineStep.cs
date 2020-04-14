using System;
using System.Collections.Generic;
using BulkWriter.Pipeline.Transforms;
using Microsoft.Extensions.Logging;

namespace BulkWriter.Pipeline.Steps
{
    /// <summary>
    /// Fluent configuration interface for an <c>EtlPipeline</c>
    /// </summary>
    /// <typeparam name="TIn">Type of input objects to this pipeline step</typeparam>
    /// <typeparam name="TOut">Type of output objects to this pipeline step</typeparam>
    public interface IEtlPipelineStep<in TIn, TOut>
    {
        /// <summary>
        /// Configures an aggregation step in the pipeline
        /// </summary>
        /// <typeparam name="TNextOut">Type of the next output object in the pipeline</typeparam>
        /// <param name="aggregator">Object that will perform the aggregation of multiple input objects to a single output</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(IAggregator<TOut, TNextOut> aggregator);

        /// <summary>
        /// Configures an aggregation step in the pipeline
        /// </summary>
        /// <typeparam name="TNextOut">Type of the next output object in the pipeline</typeparam>
        /// <param name="aggregationFunc">Func that performs the aggregation of multiple input objects to a single output</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(Func<IEnumerable<TOut>, TNextOut> aggregationFunc);

        /// <summary>
        /// Configures a pivot step in the pipeline
        /// </summary>
        /// <typeparam name="TNextOut">Type of the next output object in the pipeline</typeparam>
        /// <param name="pivot">Object that will pivot each input object to multiple output objects</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(IPivot<TOut, TNextOut> pivot);

        /// <summary>
        /// Configures a pivot step in the pipeline
        /// </summary>
        /// <typeparam name="TNextOut">Type of the next output object in the pipeline</typeparam>
        /// <param name="pivotFunc">Func that will pivot each input object to multiple output objects</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(Func<TOut, IEnumerable<TNextOut>> pivotFunc);

        /// <summary>
        /// Configures a projection step in the pipeline
        /// </summary>
        /// <typeparam name="TNextOut">Type of the next output object in the pipeline</typeparam>
        /// <param name="projector">Object that will project an input object to a new output type</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(IProjector<TOut, TNextOut> projector);

        /// <summary>
        /// Configures a projection step in the pipeline
        /// </summary>
        /// <typeparam name="TNextOut">Type of the next output object in the pipeline</typeparam>
        /// <param name="projectionFunc">Func that will project an input object to a new output type</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(Func<TOut, TNextOut> projectionFunc);

        /// <summary>
        /// Configures a transform step in the pipeline
        /// </summary>
        /// <param name="transformers">One or more objects that will transform input objects in place</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TOut> TransformInPlace(params ITransformer<TOut>[] transformers);

        /// <summary>
        /// Configures a transform step in the pipeline
        /// </summary>
        /// <param name="transformActions">One or more actions that will transform input objects in place</param>
        /// <returns>Next step in the pipeline to be configured</returns>
        IEtlPipelineStep<TOut, TOut> TransformInPlace(params Action<TOut>[] transformActions);

        /// <summary>
        /// Enables logging of the ETL pipeline internals
        /// </summary>
        /// <param name="loggerFactory">Factory used to create a new ILogger</param>
        /// <returns>Current step in the pipeline</returns>
        IEtlPipelineStep<TIn, TOut> LogWith(ILoggerFactory loggerFactory);

        /// <summary>
        /// Configures the pipeline to write its output to a BulkWriter object; finalizes the pipeline.
        /// </summary>
        /// <param name="bulkWriter">The <c>BulkWriter</c> object to which the pipeline will output</param>
        /// <returns>A pipeline object that can be executed</returns>
        IEtlPipeline WriteTo(IBulkWriter<TOut> bulkWriter);
    }
}