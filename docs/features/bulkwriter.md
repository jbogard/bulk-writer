---
layout: default
title: BulkWriter
parent: Features
nav_order: 1
---
# BulkWriter

The `BulkWriter<TResult>` class is (naturally) the main attraction of this library. It implements the streaming from `IEnumerable` to `SqlBulkCopy`, as described in [Motivation](../motivation.md). The class will automatically handle mapping of your DTOs, so the only configuration you need to provide is a database connection (or connection string) and an input `IEnumerable`.

The following constructors are provided:

```csharp
public BulkWriter(string connectionString)
public BulkWriter(string connectionString, SqlBulkCopyOptions options)
public BulkWriter(SqlConnection connection, SqlTransaction transaction = null)
public BulkWriter(SqlConnection connection, SqlBulkCopyOptions options, SqlTransaction transaction = null)
```

**Note:** `BulkWriter` is `IDisposable` and *must* be disposed properly!

## Entity Mapping

`BulkWriter` uses standard Data Annotations from the `System.ComponentModel.DataAnnotations.Schema` [namespace](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema?view=netcore-3.1) to map your DTO to tables and columns. In the absence of attributes on a property or class, the name of that property or class is used when mapping to your output table.

The following attributes are supported:

- TableAttribute
- ColumnAttribute
- KeyAttribute
- MaxLengthAttribute
- NotMappedAttribute

## Example

```csharp
[Table(Name = "MyTable")]
public class MyClass
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    public decimal WeightInKg { get; set; }

    [NotMapped]
    public bool CommunicatesSecurely { get; set; }
}

//using connection string
using (var writer = new BulkWriter<MyClass>(connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg = 80, CommunicatesSecurely = true });
    writer.WriteToDatabase(items);
}

//with custom SqlBulkCopyOptions
var bulkCopyOptions = new SqlBulkCopyOptions
{
    BulkCopyTimeout = 0,
    BatchSize = 10000
};
using (var bulkWriter = new BulkWriter<MyClass>(connectionString, bulkCopyOptions))
{
    var items = Enumerable.Range(1, 1000000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg = 80, CommunicatesSecurely = true });
    writer.WriteToDatabase(items);
}

//using direct SQL connection
using (var connection = new SqlConnection(_connectionString))
{
    var items = Enumerable.Range(1, 1000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg = 80, CommunicatesSecurely = true });
    await connection.OpenAsync();

    using var writer = new BulkWriter<MyClass>(connection);
    writer.WriteToDatabase(items);
}

//inside a transaction
var bulkCopyOptions = new SqlBulkCopyOptions
{
    BulkCopyTimeout = 0,
    BatchSize = 10000
};
using (var connection = new SqlConnection(_connectionString))
{
    var items = Enumerable.Range(1, 1000000).Select(i => new MyClass { Id = i, Name = "Bob", WeightInKg = 80, CommunicatesSecurely = true });
    await connection.OpenAsync();

    using var transaction = connection.BeginTransaction();
    using var writer = new BulkWriter<MyClass>(connection, bulkCopyOptions, transaction);
    await writer.WriteToDatabaseAsync(items);
}
```
