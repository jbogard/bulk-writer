---
layout: default
title: Pipelining
parent: Examples
nav_order: 2
---
# Pipelining

The example above shows a transform that's pretty simple. But some ETL jobs require transforms that are so complex (and the code is so complicated), it's easier to reason about and implement the transforms in steps. You can create these steps using a single LINQ query (which are actually pipelines themselves), or you can implement a Pipeline with Stages.

Typical pipelines push data from one stage to the next.

            =========         =========         =========         ==========
            |       |         |       |         |       |         |        |
    Push -> | Stage | Push -> | Stage | Push -> | Stage | Push -> |  Sink  |
            |       |         |       |         |       |         |        |
            =========         =========         =========         ==========

Using Bulk Writer, data is *pulled* through the Pipeline.

    =========         =========         =========         ==========
    |       |         |       |         |       |         |        |
    | Stage | <- Pull | Stage | <- Pull | Stage | <- Pull |  Sink  |
    |       |         |       |         |       |         |        |
    =========         =========         =========         ==========

Take the following example:

```csharp
var begin = DoStepBegin();
var step1 = DoStep1(begin);
var step2 = DoStep2(step1);
var step3 = DoStep3(step2);
DoStepEnd(step3);

// Or a one-liner!!
// DoStepEnd(DoStep3(DoStep2(DoStep1(DoStepBegin()))));

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
      foreach(var many in GetManyItems(item)) {
         yield return Step3Result(many);
      }
   }
}

private static void DoStepEnd(IEnumerable<Step3Result> input) {
   var mapping = MapBuilder.BuildAllProperties<Step3Result>();
   using (var bulkWriter = mapping.CreateBulkWriter(connectionString))
   {
      bulkWriter.WriteToDatabase(input);
   }
}
```
