---
layout: default
title: Using SqlBulkCopy
parent: SqlBulkCopy
nav_order: 1
---
# Using SqlBulkCopy

The Bulk Writer core assembly has an `IDataReader` implementation that wraps an `IEnumerator` called `EnumerableDataReader`. You can give an instance of `EnumerableDataReader` to an instance of `SqlBulkCopy`, so that when `SqlBulkCopy` calls for the next record from the `EnumerableDataReader` instance, it is retrieving the next record from the underlying `IEnumerator`.

It is conceivable that `IEnumerator.MoveNext()` and `IEnumerator.Current` are proffering records from any type of data source, but you are typically enumerating over an enumerable by retrieving an instance of `IEnumerator` by calling `IEnumerable.GetEnumerator()`. So, you can think of `EnumerableDataReader` in this way:

> **You can give `EnumerableDataReader` to `SqlBulkCopy`, and in turn, `SqlBulkCopy` will stream the data from the `IEnumerable` into your target data store.**

Most of the other code in the core assembly is for mapping properties on source data (objects yielded from an `IEnumerable`) to columns in the target data store. 

This technique does require you to reason differently about your ETL jobs: Most jobs *push* data into the target data store. This technique requires you to think about how to structure your transforms so that data is *pulled* from your source data through your transforms instead.

It is technically possible to produce infinite data sets with `IEnumerable`, which can be pulled into a SQL Server table as soon as your `IEnumerable` can produce them while using very little memory.
