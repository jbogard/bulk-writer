---
layout: default
title: Transformations
parent: Examples
nav_order: 4
---
# Transformations

With transformations we can manipulate data prior to writing to the data store.

## Aggregate
```Aggregate``` can take multiple records and output a single record.

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
### Output
```
82,000
```

## Pivot

```Pivot``` can turn one record into many.

```csharp
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var idCounter = 0;
    var items = Enumerable.Range(1, 3).ToList();
    var pipeline = EtlPipeline.StartWith(items)
        .Pivot(i =>
        {
            var result = new List<MyClass>();
            for (var j = 0; j < i; j++)
            {
                ++idCounter;
                result.Add(new MyClass { Id = idCounter, Name = $"Bob {idCounter}" });
            }
            return result;
        })
        .WriteTo(writer);

    await pipeline.ExecuteAsync();
}
```
### Output
```
Id       Name
--------------
1        Bob 1
2        Bob 2
3        Bob 3
4        Bob 4
5        Bob 5
6        Bob 6
```

## Project

```Project``` can translate your current type into a new type.

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
### Output
```
Id       Name
-----------------
1        Bob 1
2        Bob 2
3        Bob 3
...
998      Bob 998
999      Bob 999
1000     Bob 1000
```

## Transform

```Transform``` can apply changes in-place.

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
### Output
```
Id       Name    WeightInKg    WeightInLbs
------------------------------------------
1        Bob     80            176.4
2        Bob     80            176.4
3        Bob     80            176.4
...
998      Bob     80            176.4
999      Bob     80            176.4
1000     Bob     80            176.4
```
