---
layout: default
title: EnumerableDataReader
parent: Features
nav_order: 2
---
# EnumerableDataReader

`EnumerableDataReader<TResult>` is a helper class for mapping an `IEnumerable` to be readable via the `IDataReader` interface. Generally you'd only use this if you want to create your own custom mapping logic between the entities in your `IEnumerable` and `SqlBulkCopy` directly.

Its constructor signature is:

```csharp
EnumerableDataReader(IEnumerable<TResult> items, IEnumerable<PropertyMapping> propertyMappings)
```

You'll need to populate `PropertyMapping` objects for the properties you want to map from `TResult` into your data store via `SqlBulkCopy`. This exercise is left to the reader, though the `BulkWriter.Internal.TypeExtensions.BuildMappings` extension method may give you some hints.
