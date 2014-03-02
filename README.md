***Work in progress...***

Bulk Writer
===========

# Table of Contents #

1. Introduction
2. Examples
 1. Relative distances between an entity and all zip codes

# Introduction #

We've all had reasons to write ETL jobs in C# rather than Integration Services in SQL Server. Sometimes it's just because it's easier to write them in C#, sometimes it's for other reasons. For whatever reason, it's okay.

When writing an ETL process in C#, we use the tools available to us. Unfortunately those tools help us read (or stream) from source data pretty well, but they leave us writing to our target data stores with INSERT statements. What we were looking for is a way to stream data into a target data store, just as we're able to stream from source data. A technique like this would be as fast as our transforms would allow, and use very little memory compared to existing techniques.

`SqlBulkCopy` (or the Oracle or MySql equivalents) is the most likely candidate to stream into a target data store, and it is a great tool that *is* available to us, but its `WriteToServer()` methods take a `DataRow[]`, a `DataTable` or an `IDataReader`. The methods that take a `DataRow[]` or a `DataTable` aren't really useful when transforms produce very large data sets because they force us to load the entire data set into memory before being streamed into the target data store.

Which leaves us to examine how to leverage the `WriteToServer(IDataReader)` method. If you think about how `IDataReader` works, users of an `IDataReader` instance must call the `Read()` method before examining the current record held by the instance. A user advances the current record until `Read()` returns false, at which point the stream is finished and there is no longer a current record. In this way, `IDataReader` is a *non-caching forward-only reader*.

There are other non-caching forward-only readers in .NET, which are used every day by most developers. The most used example of this type of reader is `IEnumerator`, which works similarly to `IDataReader`. However, instead of a `Read():bool` method, `IEnumerator` has a `MoveNext():bool` method.

The Bulk Writer core assembly has an `IDataReader` implementation that wraps an `IEnumerator` so that when users of the `IDataReader` instance call for the next record, the implementation retrieves the next record from the underlying `IEnumerator`. Most of the other code is for  mapping properties on the source data to columns in the target data store. You can give the `IDataReader` implementation to `SqlBulkCopy`, and it'll stream your `IEnumerable` into your target data store.

Since it is technically possible to produce infinite data sets with `IEnumerable`, you can write any number of rows into a SQL Server table as soon as your `IEnumerable` can produce them and using very little memory.

# Examples #

By itself, Bulk Writer is a pretty simple concept and the code itself isn't really all that complicated. However, we believe even simple implementations can enable very complex scenarios. The rest of this document shows examples of what you can do with Bulk Writer.

## Relative distances between an entity and all zip codes ##

Suppose that for performance reasons you wanted to cache the distances between an entity (such as a store, house or distribution center) and the centroid of every zip code in the U.S. Sure, these entity locations and zip code centroids aren't likely to change often enough to warrant computing this result set on every ETL job run, but the real point of this example is to show

1. Sometimes our transforms can produce exponentially larger data sets than our source data
2. That those very large data sets can be written to our target data store much faster than by generating an INSERT statement for each row.

To be continued...