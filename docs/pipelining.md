---
layout: default
title: Pipelining
parent: Overview
nav_order: 4
---
# Pipelining

Some ETL jobs require transforms that are so complex (and the code is so complicated), it's easier to reason about and implement the transforms in steps. You can create these steps using a single LINQ query (which are actually pipelines themselves), or you can implement a Pipeline with Stages.

Typical pipelines push data from one stage to the next.

            =========         =========         =========         ==========
            |       |         |       |         |       |         |        |
    Push -> | Stage | Push -> | Stage | Push -> | Stage | Push -> |  Sink  |
            |       |         |       |         |       |         |        |
            =========         =========         =========         ==========

Using *Bulk Writer*, data is *pulled* through the Pipeline.

    =========         =========         =========         ==========
    |       |         |       |         |       |         |        |
    | Stage | <- Pull | Stage | <- Pull | Stage | <- Pull |  Sink  |
    |       |         |       |         |       |         |        |
    =========         =========         =========         ==========

See the [basic pipelining](./examples/pipelining.md) code for an example implementation.

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

This pipeline structure is precisely what's implemented by the `EtlPipeline` [class](./features/etlpipeline.md). See the [advanced pipelining](./examples/advanced-pipelining.md) code for an example implementation.
