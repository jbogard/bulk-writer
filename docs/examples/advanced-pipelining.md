---
layout: default
title: Advanced Pipelining
nav_order: 3
parent: Examples
---
# Advanced Pipelining

To implement an advanced pipeline as described in the [Pipelining Overview](../pipelining.md#advanced-pipelining), you can make use of the `EtlPipeline` [class](../features/etlpipeline.md). The main difference between this example and the basic [Pipelining](./pipelinig.md) example is that the `EtlPipeline` class buffers output from each step in a producer/consumer collection. If any of your steps implement long-running operations, this buffering helps ensure downstream steps in the pipeline can stay busy (assuming they have work in their input buffers).

```csharp
public class MyEntity
{
   public int Id { get; set; }
   public string Name { get; set; }
}

public class MyOtherEntity
{
   public int Id { get; set; }
   public string FirstName { get; set; }
   public string LastName { get; set; }
}

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

using (var writer = new BulkWriter<PipelineTestsOtherTestClass>())
{
    var items = Enumerable.Range(1, 1000000).Select(i => new MyEntity { Id = i, Name = "Carol" });
    var pipeline = EtlPipeline
        .StartWith(items)
        .Aggregate(f => f.Max(c => c.Id))
        .Pivot(new BobFromIdPivot())
        .Project(i =>
        {
            var nameParts = i.Name.Split(' ');
            return new MyOtherEntity {Id = i.Id, FirstName = nameParts[0], LastName = nameParts[1] };
        })
        .TransformInPlace(i =>
        {
            i.Id -= 1;
            i.FirstName = "Alice";
            i.LastName = $"{i.Id}";
        })
        .LogWith(loggerFactory)
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```
