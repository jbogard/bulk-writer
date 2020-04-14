---
layout: default
title: Overview
has_children: true
nav_order: 1
---
# Bulk Writer

A small library which facilitates building fast, memory-efficient, pull-based ETL processes in C#.

*Bulk Writer* provides a wrapper over `Microsoft.Data.SqlClient.SqlBulkCopy` to enable streaming of records to a target data store using an `IEnumerable` as the data source. This approach keeps memory overhead low when loading a large volumes of data, while taking advantage of high-speed bulk inserts to the database provided by `SqlBulkCopy`. Helper classes provide means to apply this model manually, and to build custom ETL pipelines to transform data on its way into the data store.
