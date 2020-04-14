---
layout: default
title: Bulk Inserting
parent: Overview
nav_order: 3
---
# Bulk Inserting

There are a number of approaches to inserting large amounts of data into the database, including batching inserts, table value parameters, and `SqlBulkCopy`. Each of these approaches is valid for certain use cases (see [https://docs.microsoft.com/en-us/azure/sql-database/sql-database-use-batching-to-improve-performance](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-use-batching-to-improve-performance) for a good breakdown). This library was written with very large data sets in mind, where we have many thousands of records (or more) to insert all at once.

## SqlBulkCopy

We use `SqlBulkCopy` (or the Oracle or MySql equivalents) in *Bulk Writer* as it typically provides the best throughput for very large record sets, but its `WriteToServer()` methods take a `DataRow[]`, a `DataTable` or an `IDataReader`. In order to keep our process memory efficient, we'd like to be able to stream our data set into the data store.

The methods that take a `DataRow[]` or a `DataTable` aren't really useful when transforms produce very large data sets because they force us to load the entire data set into memory before writing into the target data store.

That leaves us to examine how to leverage the `WriteToServer(IDataReader)` method. If you think about how `IDataReader` works, users of an `IDataReader` instance must call the `Read()` method before examining a current record. A user advances through the result set until `Read()` returns false, at which point the stream is finished and there is no longer a current record. It has no concept of a previous record. In this way, `IDataReader` is a *non-caching forward-only reader*.

There are other non-caching forward-only readers in .NET, which are used every day by most developers. The most-used example of this type of reader is `IEnumerator`, which works similarly to `IDataReader`. However, instead of a `bool Read()` method, `IEnumerator` has a `bool MoveNext()` method. Conceptually, `IDataReader` and `IEnumerator` are similar.

## Streaming data to SqlBulkCopy

The Bulk Writer core assembly has an `IDataReader` implementation that wraps an `IEnumerator` called `EnumerableDataReader`. An instance of `EnumerableDataReader` is passed to an instance of `SqlBulkCopy`, so that when `SqlBulkCopy` calls for the next record from the `EnumerableDataReader` instance, it is retrieving the next record from the underlying `IEnumerator`.

It is conceivable that `IEnumerator.MoveNext()` and `IEnumerator.Current` are proffering records from any type of data source, but you are typically enumerating over an enumerable by retrieving an instance of `IEnumerator` by calling `IEnumerable.GetEnumerator()`. So, you can think of `EnumerableDataReader` in this way:

> **`EnumerableDataReader` is given to `SqlBulkCopy`, and in turn, `SqlBulkCopy` will stream the data from the `IEnumerable` into your target data store.**

This technique does require you to reason differently about your ETL jobs: Most jobs *push* data into the target data store. This technique requires you to think about how to structure your transforms so that data is *pulled* from your source data through your transforms instead.

It is technically possible to produce infinite data sets with `IEnumerable`, which can be pulled into a SQL Server table as soon as your `IEnumerable` can produce them while using very little memory.
