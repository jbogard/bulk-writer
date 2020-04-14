---
layout: default
title: Pipelining
parent: Examples
nav_order: 2
---
# Pipelining

This example manually implements a basic pipeline as described in the [Pipelining Overview](../pipelining.md#pipelining). You can see the `IEnumerable` objects being chained together through each method call. Since we never trigger evaluation of the `IEnumerable` objects until the call to `BulkWriter.WriteToDatabase()`, we don't end up waiting on or using up memory for all 1,000,000 objects before we write to the database via `SqlBulkCopy`.

This code effectively implements the same pipeline as the [Advanced Pipelining](./advanced-pipelining.md) example, except we don't have buffers between each step, as show in the [Pipelining Overview](../pipelining.md)

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

var entities = GetEntities();
var maxId = GetMaxId(entities);
var bobObjects = CreateBobObjects(maxId);
var otherBobObjects = ProjectBobObjects(bobObjects);
var aliceObjects = TransformToAliceObjects(otherBobObjects);
WriteToDatabase(aliceObjects);

// Or a one-liner!!
// WriteToDatabase(TransformToAliceObjects(ProjectBobObjects(CreateBobObjects(GetMaxId(GetEntities())))));

private static IEnumerable<MyEntity> GetEntities()
{
   foreach (var item in Enumerable.Range(0, 1000000))
   {
      yield return new MyEntity { Id = item, Name = "Carol" };
   }
}

private static int GetMaxId(IEnumerable<MyEntity> input)
{
    return input.Max(i => i.Id);
}

private static IEnumerable<MyEntity> CreateBobObjects(int numberOfBobs)
{
   for (var j = 1; j <= numberOfBobs; j++)
   {
      yield return new MyEntity { Id = j, Name = $"Bob {j}" };
   }
}

private static IEnumerable<MyOtherEntity> ProjectBobObjects(IEnumerable<MyEntity> input)
{
   foreach (var item in input)
   {
      var nameParts = item.Name.Split(' ');
      yield return new MyOtherEntity {Id = item.Id, FirstName = nameParts[0], LastName = nameParts[1] };
   }
}

private static IEnumerable<MyOtherEntity> TransformToAliceObjects(IEnumerable<MyOtherEntity> input)
{
   foreach (var item in input)
   {
      yield return new MyOtherEntity {Id = item.Id, FirstName = "Alice", LastName = item.LastName };
   }
}

private static void WriteToDatabase(IEnumerable<MyOtherEntity> input)
{
   using (var bulkWriter = new BulkWriter<MyOtherEntity>(connectionString))
   {
      bulkWriter.WriteToDatabase(input);
   }
}
```
