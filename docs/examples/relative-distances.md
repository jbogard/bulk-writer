---
layout: default
title: LINQ to SqlBulkCopy
parent: Examples
nav_order: 1
---
# LINQ to SqlBulkCopy

Suppose that for performance reasons you wanted to cache the distances between some entities (such as a store, house or distribution center) and the centroid of every zip code in the U.S. Depending on the number of entities, you could easily produce a very large data set from these calculations. But sure, entity locations and zip code centroids aren't likely to change often enough to warrant computing this result set on every ETL job run, but the real point of this example is to show

1. Sometimes our transforms can produce exponentially larger data sets than our source data. In this example, the result set size is (<span style="white-space: nowrap;"># of Entities &times; # of Zipcodes</span>).
2. That those very large data sets can be written to our target data store much faster than by generating an `INSERT` statement for each row. In this example, you'd have to generate (<span style="white-space: nowrap;"># of Entities &times; # of Zipcodes</span>) `INSERT` statements which will never perform as well as bulk loading the data instead.

We start off with this LINQ query that serves as our transform and will produce our large data set.

```csharp
var q =
   from entity in GetAllEntities()
   where entity.IsActive && SomeOtherPredicate(entity)
   from zipCode in GetAllZipCodes()
   where zipCode.IsInContiguousStates && SomeOtherPredicate(zipCode)
   let distance = GetDistance(entity, zipCode)
   let arbitraryData = CreateSomeArbitraryData(entity, zipCode)
   where distance > 0
   select new EntityToZipCodeDistance
   {
      EntityId = entity.Id,
      ZipCode = zipCode.Zip,
      Distance = distance,
      ArbitraryData = arbitraryData
   };
```

Note that this LINQ query does not execute until the `MoveNext()` method is called on its enumerator, which will ultimately be called by `SqlBulkCopy`.

Next, all there is to do is let Bulk Writer write the results to your database table.

```csharp
using (var bulkWriter = new BulkWriter<EntityToZipCodeDistance>(connectionString))
{
    bulkWriter.WriteToDatabase(q);
}
// or async

using (var bulkWriter = new BulkWriter<EntityToZipCodeDistance>(connectionString))
{
    await bulkWriter.WriteToDatabaseAsync(q);
}
```
