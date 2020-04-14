---
layout: default
title: Motivation
parent: Overview
nav_order: 2
---
# Motivation

We've all had reasons to write ETL jobs in C# rather than with Integration Services in SQL Server. Sometimes it's because your ETL's transform logic is easier to reason about in C#, sometimes we want to utilize .NET's rich library ecosystem, but for whatever reason, it's a perfectly acceptable way to to do things.

## Challenge

When writing an ETL process in C#, we use the tools available to us like NHibernate, Entity Framework or Dapper to read from and write to databases. These tools help us stream _from_ source data pretty easily, but unfortunately, they don't make it easy to stream data _to_ our target data stores. Instead, they typically leave us writing to target data stores with `INSERT` statements, which is not performant for transforms that generate very large data sets.

> **What we need for transforms that generate very large data sets is a technique to stream data into a target data store, just as we're able to stream from source data.**

Such a technique would allow writing to target data stores as fast as our transforms and hardware will allow, compared to relying on our ORM to generate `INSERT` statements. In most cases, it would also use significantly less memory.

## Solution

This library and the guidance that follows show how to use `SqlBulkCopy`, `IEnumerable` and `IDataReader` to enable this kind of streaming technique, that is, to stream from a data source and to stream into a data store, with our C# ETLs.  We'll also cover how to change your "push"-based transforms that use `INSERT` statements to "pull"-based transforms that use `IEnumerable`.
