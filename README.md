***Work in progress...***

Bulk Writer
===========

We've all had reasons to write ETL processes in C# rather than Integration Services in SQL Server. Sometimes it's just because it's easier to write them in C#, sometimes it's for other reasons. For whatever reason, it's okay.

When writing an ETL process in C#, we use the tools available to us. Unfortunately those tools will help us read (or stream) from source data pretty well, but they leave us writing to our target data stores with INSERT statements. What we've lacked in our C# ETL jobs is a way to stream from source data, do our transforms on that stream and then stream the result set into a target data store.

To be continued...