---
layout: default
title: EtlPipeline
parent: Features
nav_order: 3
---
# EtlPipeline

The `EtlPipeline` class exposes a fluent interface for configuring your processing pipeline. The pipeline configuration will look roughly like this:

- [Start the Pipeline](#starting-the-pipeline-configuration)
- Apply optional [transformations](#applying-transformations)
- Apply optional [configuration](#configure-logging)
- [Write to BulkWriter](#writing-to-bulkwriter)
- [Execute the Pipeline](#execute-the-pipeline)
- [Handle Errors](#handle-errors)

See the [Advanced Pipelining example](../examples/advanced-pipelining.md) for a full pipeline implementation.

## Starting the pipeline configuration

 Start pipeline configuration with a call to

 ```csharp
 IEtlPipelineStep<T, T> StartWith<T>(IEnumerable<T> input)
 ```

## Applying Transformations

With transformations we can manipulate data prior to writing to the data store.

### Aggregate

`Aggregate` takes multiple records and outputs a single record.

```csharp
IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(IAggregator<TOut, TNextOut> aggregator);
IEtlPipelineStep<TOut, TNextOut> Aggregate<TNextOut>(Func<IEnumerable<TOut>, TNextOut> aggregationFunc);
```

**Example:**

```csharp
using (var writer = new BulkWriter<int>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg = 82 });
    var pipeline = EtlPipeline.StartWith(items)
        .Aggregate(f => f.Sum(c => c.WeightInKg))
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```

**Output:**

`82,000`

### Pivot

`Pivot` turns one record into many.

```csharp
IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(IPivot<TOut, TNextOut> pivot);
IEtlPipelineStep<TOut, TNextOut> Pivot<TNextOut>(Func<TOut, IEnumerable<TNextOut>> pivotFunc);
```

**Performance Note:**
It's not possible to `yield return` from an anonymous method in C#. Since the `Pivot` method returns an `IEnumerable`, you'll almost certainly want to write a class that implements the `IPivot` interface rather than pass in a `Func`. Otherwise, you may lose the benefit of streaming records through the `IEnumerable` rather that create them all in memory before your step can write to its output collection.

**Example:**

```csharp
public class BobFromIdPivot : IPivot<int, MyEntity>
{
   public IEnumerable<MyEntity> Pivot(int i)
   {
      for (var j = 1; j <= i; j++)
      {
         yield return new MyEntity { Id = j, Name = $"Bob {j}" };
      }
   }
}

using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var idCounter = 0;
    var items = Enumerable.Range(1, 3).ToList();
    var pipeline = EtlPipeline.StartWith(items)
        .Pivot(new BobPivot())
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```

**Output:**

| Id  | Name  |
|:----|:------|
| 1   | Bob 1 |
| 2   | Bob 2 |
| 3   | Bob 3 |
| 4   | Bob 4 |
| 5   | Bob 5 |
| 6   | Bob 6 |

### Project

`Project` can translate your current type into a new type.

```csharp
IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(IProjector<TOut, TNextOut> projector);
IEtlPipelineStep<TOut, TNextOut> Project<TNextOut>(Func<TOut, TNextOut> projectionFunc);
```

**Example:**

```csharp
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyOtherClass { Id = i, FirstName = "Bob", LastName = $"{i}" );
    var pipeline = EtlPipeline
        .StartWith(items)
        .Project(i => new MyClass { Id = i.Id, Name = $"{i.FirstName} {i.LastName}" })
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```

**Output:**

| Id   | Name     |
|:-----|:---------|
| 1    | Bob 1    |
| 2    | Bob 2    |
| 3    | Bob 3    |
| ...  | ...      |
| 998  | Bob 998  |
| 999  | Bob 999  |
| 1000 | Bob 1000 |

### Transform

`Transform` applies changes to objects in-place as they stream through. Multiple transforms may be applied in a single step, if desired.

```csharp
IEtlPipelineStep<TOut, TOut> TransformInPlace(params ITransformer<TOut>[] transformers);
IEtlPipelineStep<TOut, TOut> TransformInPlace(params Action<TOut>[] transformActions);
```

**Example:**

```csharp
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg =  80 });
    var pipeline = EtlPipeline
        .StartWith(items)
        .TransformInPlace(i =>
        {
            i.WeightInLbs = i.WeightInKg * 2.205;
        })
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```

**Output:**

| Id   | Name | WeightInKg | WeightInLbs |
|:-----|:-----|:-----------|-------------|
| 1    | Bob  | 80         | 176.4       |
| 2    | Bob  | 80         | 176.4       |
| 3    | Bob  | 80         | 176.4       |
| ...  | ...  | ...        | ...         |
| 998  | Bob  | 80         | 176.4       |
| 999  | Bob  | 80         | 176.4       |
| 1000 | Bob  | 80         | 176.4       |

## Configure Logging

Logging is configured via the `LogWith(ILoggerFactory)` method, where `ILoggerFactory` is from the `Microsoft.Extensions.Logging` library.

```csharp
IEtlPipelineStep<TIn, TOut> LogWith(ILoggerFactory loggerFactory);
```

This will log the start, stop and any exceptions thrown by each step in your pipeline. If you need logging inside the code you provide to actually transform your data, you should either capture a logger instance in each `Action` or `Func` passed to your pipeline config, or add logger instances inside your implementations of the transform interfaces.

## Writing to BulkWriter

Finish up pipeline configuration by calling `WriteTo`

```csharp
IEtlPipeline WriteTo(IBulkWriter<TOut> bulkWriter);
```

**Example:**

```csharp
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg =  80 });
    var pipeline = EtlPipeline
        .StartWith(items)
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```

## Execute the Pipeline

After calling the `WriteTo(BulkWriter)` method, you'll have an instance of an `IEtlPipeline` object. Execute the pipeline by calling `ExecuteAsync`. Each step in your pipeline (including `StartWith` and `WriteTo`) will run in its own separate `Task`. The task returned by the call to `ExecuteAsync` will wait for all of the child tasks to complete before returning.

```csharp
Task ExecuteAsync();
Task ExecuteAsync(CancellationToken cancellationToken);
```

**Example:**

```csharp
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg =  80 });
    var pipeline = EtlPipeline
        .StartWith(items)
        .WriteTo(writer);

    await pipeline.ExecuteAsync(cancellationToken);
}
```

## Handle Errors

Since each step in the pipeline runs under the parent task returned by `ExecuteAsync`, you can examine the parent `Task.Exception.InnerExceptions` property for all exceptions that may have been thrown when the pipeline was executed. Any records that streamed through to the `BulkWriter` before the exception halted the pipeline will be written to the database.

```csharp
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg =  80 });
    var pipeline = EtlPipeline
        .StartWith(items)
        .WriteTo(writer);

    var pipelineExecutionTask = pipeline.ExecuteAsync(cancellationToken);
    try
    {
        await pipelineExecutionTask;
    }

    catch (Exception e)
    {
        //e will contain the first exception thrown by a pipeline step

        //pipelineExecutionTask.Exception is of type AggregateException
        //Its InnerException property will have all exceptions that were thrown
    }
}
```
