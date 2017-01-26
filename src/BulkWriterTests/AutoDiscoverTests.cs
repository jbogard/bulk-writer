using System;
using System.Collections.Generic;
using BulkWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BulkWriter.Tests
{
    [TestClass]
    public class AutoDiscoverTests
    {
        [TestMethod]
        public void Discovers_Quoted_Table_Name()
        {
            string tableName = BulkWriter.AutoDiscover.TableName<MyTestClass>(true);
            StringAssert.StartsWith(tableName, "[");
            StringAssert.EndsWith(tableName, "]");
        }

        [TestMethod]
        public void Maps_Only_Appropriate_Properties()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
            string tableName = BulkWriter.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .MapProperty(x => x.Id, x => x.DoNotMap()); // Do not map this property

            IEnumerable<BulkWriter.PropertyMapping> propertyMappings = ((BulkWriter.MapBuilderContext<MyTestClass>) mapping).GetPropertyMappings();

            BulkWriter.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.PropertyMapping propertyMapping in propertyMappings)
            {
                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.IsTrue(propertyMapping.Destination.IsPropertySet((BulkWriter.MappingProperty) i));
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Fails_On_MisMatch_Column_Auto_Map()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
            string tableName = BulkWriter.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[MisMatchedColumn] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder.MapAllProperties<MyTestClass>();

            IEnumerable<BulkWriter.PropertyMapping> propertyMappings = ((BulkWriter.MapBuilderContext<MyTestClass>) mapping).GetPropertyMappings();

            try
            {
                BulkWriter.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);
            }
            finally
            {
                TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Fails_On_MisMatch_Column_Manual_Map()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
            string tableName = BulkWriter.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            var mapping = MapBuilder
                .MapAllProperties<MyTestClass>()
                .MapProperty(x => x.Name, x => x.ToColumnName("MisMatchColumn"));

            IEnumerable<BulkWriter.PropertyMapping> propertyMappings = ((BulkWriter.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            try
            {
                BulkWriter.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);
            }
            finally
            {
                TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);
            }
        }

        [TestMethod]
        public void Can_Find_Destination_Table_Manual_Map()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
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

            IEnumerable<BulkWriter.PropertyMapping> propertyMappings = ((BulkWriter.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            BulkWriter.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.PropertyMapping propertyMapping in propertyMappings)
            {
                Assert.IsTrue(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.IsTrue(propertyMapping.Destination.IsPropertySet((BulkWriter.MappingProperty)i));
                    }
                }
            }
        }

        [TestMethod]
        public void Can_Find_Column_Manual_Map()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
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

            IEnumerable<BulkWriter.PropertyMapping> propertyMappings = ((BulkWriter.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            BulkWriter.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.PropertyMapping propertyMapping in propertyMappings)
            {
                Assert.IsTrue(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.IsTrue(propertyMapping.Destination.IsPropertySet((BulkWriter.MappingProperty)i));
                    }
                }
            }
        }

        [TestMethod]
        public void Can_Find_All_Columns_Auto_Map()
        {
            const string connectionString = "Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI";
            string tableName = BulkWriter.AutoDiscover.TableName<MyTestClass>(false);

            TestHelpers.ExecuteNonQuery(connectionString,
                "CREATE TABLE [dbo].[" + tableName + "](" +
                "[Id] [int] IDENTITY(1,1) NOT NULL," +
                "[Name] [nvarchar](50) NULL," +
                "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ([Id] ASC)" +
                ")");

            IMapBuilderContext<MyTestClass> mapping = MapBuilder.MapAllProperties<MyTestClass>();

            IEnumerable<BulkWriter.PropertyMapping> propertyMappings = ((BulkWriter.MapBuilderContext<MyTestClass>)mapping).GetPropertyMappings();

            BulkWriter.AutoDiscover.Mappings(connectionString, tableName, propertyMappings);

            TestHelpers.ExecuteNonQuery(connectionString, "DROP TABLE " + tableName);

            foreach (BulkWriter.PropertyMapping propertyMapping in propertyMappings)
            {
                Assert.IsTrue(propertyMapping.ShouldMap);

                if (propertyMapping.ShouldMap)
                {
                    for (int i = 0; i < BulkWriter.MappingDestination.PropertyIndexCount; i++)
                    {
                        Assert.IsTrue(propertyMapping.Destination.IsPropertySet((BulkWriter.MappingProperty)i));
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