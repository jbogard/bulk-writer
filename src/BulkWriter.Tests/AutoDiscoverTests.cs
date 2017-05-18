using System;
using System.Collections.Generic;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class AutoDiscoverTests
    {
        [Fact]
        public void Discovers_Quoted_Table_Name()
        {
            var tableName = AutoDiscover.TableName<MyTestClass>(true);
            Assert.StartsWith("[", tableName);
            Assert.EndsWith("]", tableName);
        }

        [Fact]
        public void Maps_Only_Appropriate_Properties()
        {
            var connectionString = TestHelpers.ConnectionString;
            var tableName = AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .MapProperty(x => x.Id, x => x.DoNotMap()); // Do not map this property

            var propertyMappings = ((MapBuilderContext<MyTestClass>) mapping).GetPropertyMappings();

            AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (var propertyMapping in propertyMappings)
            {
                if (propertyMapping.ShouldMap)
                {
                    for (var i = 0; i < MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((MappingProperty) i));
                    }
                }
            }
        }

        [Fact]
        public void Fails_On_MisMatch_Column_Auto_Map()
        {
            var connectionString = TestHelpers.ConnectionString;
            var tableName = AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[MisMatchedColumn] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder.MapAllProperties<MyTestClass>();

            var propertyMappings = ((MapBuilderContext<MyTestClass>) mapping).GetPropertyMappings();

            try
            {
                Assert.Throws<InvalidOperationException>(() => AutoDiscover.Mappings(connectionString, tableName, propertyMappings));
            }
            finally
            {
                TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);
            }
        }

        [Fact]
        public void Fails_On_MisMatch_Column_Manual_Map()
        {
            var connectionString = TestHelpers.ConnectionString;
            var tableName = AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .MapProperty(x => x.Name, x => x.ToColumnName("MisMatchColumn"));

            var propertyMappings = ((MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            try
            {
                Assert.Throws<InvalidOperationException>(() => AutoDiscover.Mappings(connectionString, tableName, propertyMappings));
            }
            finally
            {
                TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);
            }
        }

        [Fact]
        public void Can_Find_Destination_Table_Manual_Map()
        {
            var connectionString = TestHelpers.ConnectionString;
            const string tableName = "TempTestTable";

            TestHelpers.ExecuteNonQuery(connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .DestinationTable(tableName);

            var propertyMappings = ((MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (var propertyMapping in propertyMappings)
            {
                Assert.True(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (var i = 0; i < MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((MappingProperty)i));
                    }
                }
            }
        }

        [Fact]
        public void Can_Find_Column_Manual_Map()
        {
            var connectionString = TestHelpers.ConnectionString;
            const string tableName = "TempTestTable";

            TestHelpers.ExecuteNonQuery(connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[ManualColumnName] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .DestinationTable(tableName)
                .MapProperty(x => x.Name, x => x.ToColumnName("ManualColumnName"));

            var propertyMappings = ((MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (var propertyMapping in propertyMappings)
            {
                Assert.True(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (var i = 0; i < MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((MappingProperty)i));
                    }
                }
            }
        }

        [Fact]
        public void Can_Find_All_Columns_Auto_Map()
        {
            var connectionString = TestHelpers.ConnectionString;
            var tableName = AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString, $"DROP TABLE IF EXISTS [dbo].[{tableName}]");

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder.MapAllProperties<MyTestClass>();

            var propertyMappings = ((MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (var propertyMapping in propertyMappings)
            {
                Assert.True(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (var i = 0; i < MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((MappingProperty)i));
                    }
                }
            }
        }

        public class MyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}