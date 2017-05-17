using System;
using System.Collections.Generic;
using BulkWriter;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class AutoDiscoverTests
    {
        [Fact]
        public void Discovers_Quoted_Table_Name()
        {
            string tableName = BulkWriter.Internal.AutoDiscover.TableName<MyTestClass>(true);
            Assert.StartsWith("[", tableName);
            Assert.EndsWith("]", tableName);
        }

        [Fact]
        public void Maps_Only_Appropriate_Properties()
        {
            string connectionString = TestHelpers.ConnectionString;
            string tableName = BulkWriter.Internal.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .MapProperty(x => x.Id, x => x.DoNotMap()); // Do not map this property

            IEnumerable<BulkWriter.Internal.PropertyMapping> propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>) mapping).GetPropertyMappings();

            BulkWriter.Internal.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.Internal.PropertyMapping propertyMapping in propertyMappings)
            {
                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.Internal.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((BulkWriter.Internal.MappingProperty) i));
                    }
                }
            }
        }

        [Fact]
        public void Fails_On_MisMatch_Column_Auto_Map()
        {
            string connectionString = TestHelpers.ConnectionString;
            string tableName = BulkWriter.Internal.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[MisMatchedColumn] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder.MapAllProperties<MyTestClass>();

            IEnumerable<BulkWriter.Internal.PropertyMapping> propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>) mapping).GetPropertyMappings();

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
            string connectionString = TestHelpers.ConnectionString;
            string tableName = BulkWriter.Internal.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .MapProperty(x => x.Name, x => x.ToColumnName("MisMatchColumn"));

            IEnumerable<BulkWriter.Internal.PropertyMapping> propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

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
            string connectionString = TestHelpers.ConnectionString;
            const string tableName = "TempTestTable";

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .DestinationTable(tableName);

            IEnumerable<BulkWriter.Internal.PropertyMapping> propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            BulkWriter.Internal.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.Internal.PropertyMapping propertyMapping in propertyMappings)
            {
                Assert.True(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.Internal.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((BulkWriter.Internal.MappingProperty)i));
                    }
                }
            }
        }

        [Fact]
        public void Can_Find_Column_Manual_Map()
        {
            string connectionString = TestHelpers.ConnectionString;
            const string tableName = "TempTestTable";

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[ManualColumnName] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .DestinationTable(tableName)
                .MapProperty(x => x.Name, x => x.ToColumnName("ManualColumnName"));

            IEnumerable<BulkWriter.Internal.PropertyMapping> propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            BulkWriter.Internal.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.Internal.PropertyMapping propertyMapping in propertyMappings)
            {
                Assert.True(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.Internal.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((BulkWriter.Internal.MappingProperty)i));
                    }
                }
            }
        }

        [Fact]
        public void Can_Find_All_Columns_Auto_Map()
        {
            string connectionString = TestHelpers.ConnectionString;
            string tableName = BulkWriter.Internal.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder.MapAllProperties<MyTestClass>();

            IEnumerable<BulkWriter.Internal.PropertyMapping> propertyMappings = ((BulkWriter.Internal.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            BulkWriter.Internal.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.Internal.PropertyMapping propertyMapping in propertyMappings)
            {
                Assert.True(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.Internal.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.True(propertyMapping.Destination.IsPropertySet((BulkWriter.Internal.MappingProperty)i));
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