# Bulk Writer

Bulk Writer is a small library which facilitates building fast, pull-based ETL processes in C# using `SqlBulkCopy`. 

## Documentation

Documentation can be found at https://headspringlabs.github.io/bulk-writer/

## Installation

[Bulk Writer](https://www.nuget.org/packages/BulkWriter/) is available on NuGet and can be installed using the package manager console:

```
PM> Install-Package BulkWriter
```

## Usage

```csharp
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

using (var bulkWriter = new BulkWriter<EntityToZipCodeDistance>(connectionString))
{
    bulkWriter.WriteToDatabase(q);
}
// or async

using (var bulkWriter = new BulkWriter<EntityToZipCodeDistance>(connectionString))
{
    await bulkWriter.WriteToDatabaseAsync(q);
}
```

## Building Locally

Run the following command once to setup your environment.
```
PS> .\setup.ps1
```

Run the command below to build and test the project.

```
PS> .\psake.cmd
```

## Contributing

Pull Requests are welcome. If you identify a bug or would like to make a feature request feel free to submit a GitHub Issue to start a discussion.