---
layout: default
title: Advanced Pipelining
nav_order: 3
parent: Examples
---
# Advanced Pipelining

The example above is fine, but we're only processing one source data item at a time. If one pipeline stage takes longer to produce output than other stages, all stage processing suffers. There are times when you'd like any pipeline stage to continue to process available items even if other stages in the pipeline are blocked. For an example, let's consider a segment of a pipeline comprised of two stages.

Suppose Stage 1 was IO-bound because it queried for and produced a result set for each of its input items. In other words, Stage 1 is producing a larger data set than its input and it may be spending a lot of its time waiting.

Next, supposed Stage 2 was CPU bound because it performed hundreds of calculations on each output produced by Stage 1. In this example, there's no reason why Stage 2 shouldn't be able to perform its calculations while Stage 1 is blocked or producing input for Stage 2.

We form a pipeline like this by running each pipeline stage on its own thread and by introducing an input and output buffer between each stage. Now, instead of a pipeline stage pulling directly from the previous stage, each pipeline stage pushes to and pulls from its input and output buffer, respectively.

Such a pipeline would look like this:

            ===                           ===                           ===
            | |         =========         | |         =========         | |         ==========
            ===         |       |         ===         |       |         ===         |        |
    Push -> | | <- Pull | Stage | Push -> | | <- Pull | Stage | Push -> | | <- Pull |  Sink  |
            ===         |       |         ===         |       |         ===         |        |
            | |         =========         | |         =========         | |         ==========
            ===                           ===                           ===
           Buffer                        Buffer                        Buffer

Since each stage is running on its own thread, we need to be careful so that the stage's thread doesn't end before all the items the pipeline needs to process have been pushed through. So, we'll need a way to indicate that previous stages have finished.

We also want to block `SqlBulkCopy` until an item is ready to write. In essence, what we need is a buffer that is thread-safe and blocks the current thread until a new item is available. .NET already has such a buffer, `BlockingCollection<T>`.

To implement a pipeline like this, you would do the following:

```csharp
var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

var stage1Input = new BlockingCollection<Stage1Input>();
var stage2Input = new BlockingCollection<Stage2Input>();
var finalStageInput = new BlockingCollection<FinalStageInput>();

var stage1 = taskFactory.StartNew(() => {
   var enumerable = stage1Input.GetConsumingEnumerable();
   
   try {
      foreach(var item in enumerable) {
         var outputs = GetOutputsFromIO(item);
         foreach(var output in outputs) {
            stage2Input.Add(output);
         }
      }
   } finally {
      // Let's the task managing Stage 2 know that it can end
      stage2Input.CompleteAdding();
   }
});

var stage2 = taskFactory.StartNew(() => {
   var enumerable = stage2Input.GetConsumingEnumerable();
   
   try {
      foreach(var item in enumerable) {
         var outputs = DoLotsOfCalculations(item);
         foreach(var output in outputs) {
            finalStageInput.Add(output);
         }
      }
   } finally {
      finalStageInput.CompleteAdding();
   }
});

var finalStage = taskFactory.StartNew(() => {
   var enumerable = finalStageInput.GetConsumingEnumerable();
   using (var bulkWriter = new BulkWriter<FinalStageInput>(connectionString))
   {
      bulkWriter.WriteToDatabase(enumerable);
   }
});

// All stages are started and waiting for work at this point

// Populate your first stage here
foreach (var item in Enumerable.Range(0, 1000000)) {
   stage1Input.add(new Stage1Input());
}

stage1Input.CompleteAdding();

await Task.WhenAll(new[] { stage1, stage2, finalStage });
```