Bulk Writer
===========

# Table of Contents #

1. Introduction
1. DecoratedModel assembly
1. Nhibernate assembly 
1. Examples
 1. Relative distances between an entity and all zip codes
 2. Pipelining
 3. Advanced Pipelining

# Introduction #

We've all had reasons to write ETL jobs in C# rather than Integration Services in SQL Server. Sometimes it's just because it's easier to write them in C#, sometimes it's for other reasons. For whatever reason, it's okay.

When writing an ETL process in C#, we use the tools available to us. Unfortunately those tools help us read (or stream) from source data pretty well, but they leave us writing to our target data stores with `INSERT` statements. What we were looking for is a way to stream data into a target data store, just as we're able to stream from source data. A technique like this would be as fast as our transforms would allow, and use very little memory compared to existing techniques.

`SqlBulkCopy` (or the Oracle or MySql equivalents) is the most likely candidate to stream into a target data store, and it is a great tool that *is* available to us, but its `WriteToServer()` methods take a `DataRow[]`, a `DataTable` or an `IDataReader`. The methods that take a `DataRow[]` or a `DataTable` aren't really useful when transforms produce very large data sets because they force us to load the entire data set into memory before being streamed into the target data store.

Which leaves us to examine how to leverage the `WriteToServer(IDataReader)` method. If you think about how `IDataReader` works, users of an `IDataReader` instance must call the `Read()` method before examining the current record held by the instance. A user advances the current record until `Read()` returns false, at which point the stream is finished and there is no longer a current record. In this way, `IDataReader` is a *non-caching forward-only client-pulling reader*.

There are other non-caching forward-only client-pulling readers in .NET, which are used every day by most developers. The most used example of this type of reader is `IEnumerator`, which works similarly to `IDataReader`. However, instead of a `Read():bool` method, `IEnumerator` has a `MoveNext():bool` method.

The Bulk Writer core assembly has an `IDataReader` implementation that wraps an `IEnumerator` so that when users of the `IDataReader` instance call for the next record, the implementation retrieves the next record from the underlying `IEnumerator`. Most of the other code is for  mapping properties on the source data to columns in the target data store. You can give the `IDataReader` implementation to `SqlBulkCopy`, and it'll stream your `IEnumerable` into your target data store.

This technique does require you to reason differently about your ETL jobs. Most jobs *push* data into the target data store. This technique requires you to think about how to structure your transforms so that data is *pulled* from your source data through your transforms instead.

Any number of rows - since it is technically possible to produce infinite data sets with `IEnumerable` - can be pulled into a SQL Server table as soon as your `IEnumerable` can produce them and using very little memory.

## DecoratedModel assembly ##

*Work in progress...*

## Nhibernate assembly ##

*Work in progress...*

# Examples #

By itself, Bulk Writer is a pretty simple concept and the code itself isn't really all that complicated. However, we believe even simple implementations can enable very complex scenarios. The rest of this document shows examples of what you can do with Bulk Writer.

All of these examples use the DecoratedModel assembly.

## Relative distances between an entity and all zip codes ##

Suppose that for performance reasons you wanted to cache the distances between an entity (such as a store, house or distribution center) and the centroid of every zip code in the U.S. Depending on the number of entities, you could easily produce a very large data set from these calculations. But sure, entity locations and zip code centroids aren't likely to change often enough to warrant computing this result set on every ETL job run, but the real point of this example is to show

1. Sometimes our transforms can produce exponentially larger data sets than our source data
2. That those very large data sets can be written to our target data store much faster than by generating an `INSERT` statement for each row.

We start off with this LINQ query that serves as our transform and will produce our large data set.

    var q =
      from entity in GetAllEntities()
      where entity.IsActive && SomeOtherPredicate(entity)
      from zipCode in GetAllZipCodes()
      where zipCode.IsInContiguousStates && SomeOtherPredicate(zipCode)
      let distance = GetDistance(entity, zipCode)
      let arbitraryData = CreateSomeArbitraryData(entity, zipCode)
      where distance > 0
      select new EntityToZipCodeDistance {
         EntityId = entity.Id,
         ZipCode = zipCode.Zip,
         Distance = distance,
         ArbitraryData = arbitraryData
      };

Note that this LINQ query does not execute until its `IEnumerator.MoveNext()` method is called, which will ultimately be called by `SqlBulkCopy`.

Next, all there is to do is let Bulk Writer write the results to your database table.

    var bulkCopyFactory = CreateBulkCopyFactory();
    var dataWriter = new EnumerableDataWriter();
    dataWriter.WriteToDatabase(q, bulkCopyFactory);

## Pipelining ##

The example above shows a transform that's pretty simple. But some ETL jobs require transforms that are so complex (and the code is so complicated), it's easier to reason about and implement the transforms in steps. You can create these steps using a single LINQ query if that works for you, or you can implement a Pipeline with Stages.

Typical pipelines look like this:

            =========     =========     =========     ==========
            |       |     |       |     |       |     |        |
    Push -> | Stage | --> | Stage | --> | Stage | --> |  Sink  |
            |       |     |       |     |       |     |        |
            =========     =========     =========     ==========

Using Bulk Writer, the Pipeline concept is a little different because data is usually *pushed* through Pipelines, but with Bulk Writer, data is *pulled* through the Pipeline by `EnumerableDataWriter`.

    =========     =========     =========         ==========
    |       |     |       |     |       |         |        |
    | Stage | <-- | Stage | <-- | Stage | <- Pull |  Sink  |
    |       |     |       |     |       |         |        |
    =========     =========     =========         ==========

Take the following example:

	private static void Main(string[] args) {
       var begin = DoStepBegin();
       var step1 = DoStep1(begin);
       var step2 = DoStep2(step1);
       var step3 = DoStep3(step2);
       DoStepEnd(step3);
      
       // Or a one-liner!!
       // DoStepEnd(DoStep3(DoStep2(DoStep1(DoStepBegin()))));

       Console.WriteLine("Press ENTER to continue...");
       Console.ReadLine();
    }

    private static IEnumerable<BeginStepResult> DoStepBegin() {
       foreach (var item in Enumerable.Range(0, 1000000)) {
          yield return new BeginStepResult();
       }
    }

    private static IEnumerable<Step1Result> DoStep1(IEnumerable<BeginStepResult> input) {
        // Or using LINQ, which does the same thing as below
        return input.Where(x => SomePredicate(x)).Select(x => new Step1Result(x));
    }

    private static IEnumerable<Step2Result> DoStep2(IEnumerable<Step1Result> input) {
       foreach (var item in input) {
	      if (SomePredicate(item)) {
             yield return new Step2Result(item);
          }
       }
    }

    private static IEnumerable<Step3Result> DoStep3(IEnumerable<Step2Result> input) {
       foreach (var item in input) {
          yield return Step3Result(item);
       }
    }

    private static void DoStepEnd(IEnumerable<Step3Result> input) {
       var bulkCopyFactory = CreateBulkCopyFactory();
       var dataWriter = new EnumerableDataWriter();
       dataWriter.WriteToDatabase(input, bulkCopyFactory);
    }

## Advanced Pipelining ##

The example above is fine, but we're only processing one source data item at a time. If one pipeline stage takes longer to produce output than other stages, all stage processing suffers. We argue that there are times when you'd like any pipeline stage to continue to process available items even if other stages in the pipeline are blocked. Such a pipeline would look like this