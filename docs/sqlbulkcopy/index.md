---
layout: default
title: SqlBulkCopy
nav_order: 1
has_children: true
---
# SqlBulkCopy

`SqlBulkCopy` (or the Oracle or MySql equivalents) is the most likely candidate to stream into a target data store, but its `WriteToServer()` methods take a `DataRow[]`, a `DataTable` or an `IDataReader`.

The methods that take a `DataRow[]` or a `DataTable` aren't really useful when transforms produce very large data sets because they force us to load the entire data set into memory before being streamed into the target data store.

Which leaves us to examine how to leverage the `WriteToServer(IDataReader)` method. If you think about how `IDataReader` works, users of an `IDataReader` instance must call the `Read()` method before examining a current record. A user advances through the result set until `Read()` returns false, at which point the stream is finished and there is no longer a current record. It has no concept of a previous record. In this way, `IDataReader` is a *non-caching forward-only reader*.

There are other non-caching forward-only readers in .NET, which are used every day by most developers. The most used example of this type of reader is `IEnumerator`, which works similarly to `IDataReader`. However, instead of a `Read():bool` method, `IEnumerator` has a `MoveNext():bool` method. Conceptually, `IDataReader` and `IEnumerator` are similar.