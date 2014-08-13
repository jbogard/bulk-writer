using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using NHibernate;
using NHibernate.Mapping;
using Configuration = NHibernate.Cfg.Configuration;
using Environment = NHibernate.Cfg.Environment;

namespace Headspring.BulkWriter.Nhibernate
{
    public class BulkCopyFactory : IBulkCopyFactory
    {
        private readonly Configuration configuration;

        public BulkCopyFactory(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public IBulkCopy Create(object item, BulkWriterOptions options, out IPropertyToOrdinalMappings mappings)
        {
            var connectionString = this.configuration.GetProperty(Environment.ConnectionString);

            if (connectionString == null)
            {
                var connectionStringName = this.configuration.GetProperty(Environment.ConnectionStringName);
                if (connectionStringName == null)
                    throw new InvalidOperationException("Unable to create SqlConnection without a valid connection string.");

                connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            }

            var connection = new SqlConnection(connectionString);
            connection.Open();

            IsolationLevel isolationLevel;
            var isolationLevelSetting = this.configuration.GetProperty(Environment.Isolation);
            var transactionIsolationLevel = Enum.TryParse(isolationLevelSetting, out isolationLevel) ? isolationLevel : options.IsolationLevel;

            var transaction = connection.BeginTransaction(transactionIsolationLevel);

            var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                BulkCopyTimeout = int.MaxValue,
                BatchSize = 250
            };

            var type = NHibernateUtil.GetClass(item);
            var mappingsImpl = new PropertyToOrdinalMappings(type);

            AddColumnMappings(type, connectionString, sqlBulkCopy, mappingsImpl, true);

            var baseType = type.BaseType;
            while (typeof(object) != baseType)
            {
                AddColumnMappings(baseType, connectionString, sqlBulkCopy, mappingsImpl, false);
                Debug.Assert(baseType != null, "baseType != null");
                baseType = baseType.BaseType;
            }

            mappings = mappingsImpl;

            return new WrappedBulkCopy(connection, transaction, sqlBulkCopy);
        }

        private void AddColumnMappings(Type type, string connectionString, SqlBulkCopy sqlBulkCopy, PropertyToOrdinalMappings mappings, bool setDestinationTableName)
        {
            PersistentClass classMapping = null;

            try
            {
                classMapping = this.configuration.GetClassMapping(type);
            }
            catch
            {
            }

            if (null == classMapping)
            {
                return;
            }

            string tableName = classMapping.Table.Name;
            if (setDestinationTableName)
            {
                sqlBulkCopy.DestinationTableName = tableName;
            }

            var schemaReader = new SchemaReader();
            var sortedSchemaRows = schemaReader.GetSortedSchemaRows(connectionString, tableName);

            if (setDestinationTableName)
            {
                MapDisriminator(classMapping, sortedSchemaRows, mappings, sqlBulkCopy);
            }

            MapProperties(classMapping, sortedSchemaRows, mappings, sqlBulkCopy);
        }

        private void MapDisriminator(PersistentClass classMapping, IEnumerable<DbSchemaRow> sortedSchemaRows, PropertyToOrdinalMappings mappings, SqlBulkCopy sqlBulkCopy)
        {
            string descriminatorValue = classMapping.DiscriminatorValue;
            if (string.IsNullOrWhiteSpace(descriminatorValue))
            {
                return;
            }

            var discriminator = classMapping.Discriminator;
            if (discriminator != null)
            {
                var column = discriminator.ColumnIterator.OfType<Column>().Single();

                string columnName = column.Name;
                var matchingSchemaRow = sortedSchemaRows.First(row => string.Equals(row.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));

                int ordinal;
                mappings.Add("class", descriminatorValue, out ordinal);

                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping
                {
                    SourceOrdinal = ordinal,
                    DestinationOrdinal = matchingSchemaRow.ColumnOrdinal
                });
            }
        }

        private void MapProperties(PersistentClass classMapping, DbSchemaRow[] sortedSchemaRows, PropertyToOrdinalMappings mappings, SqlBulkCopy sqlBulkCopy)
        {
            foreach (var property in classMapping.PropertyIterator)
            {
                // We don't have to worry about identifier properties because PropertyIterator doesn't contain them

                if (!PropertyToOrdinalMappings.ShouldMap(property))
                {
                    continue;
                }

                // We're only supporting 1 property -> 1 column mapping
                var column = property.ColumnIterator.OfType<Column>().Single();
                string columnName = column.Name;
                try
                {
                    var matchingSchemaRow = sortedSchemaRows.First(row => string.Equals(row.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));

                    int ordinal;
                    mappings.Add(property, this.configuration, out ordinal);

                    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping
                    {
                        SourceOrdinal = ordinal,
                        DestinationOrdinal = matchingSchemaRow.ColumnOrdinal
                    });
                }
                catch (InvalidOperationException error)
                {
                    throw new InvalidOperationException(string.Format("Failed to map the Class {0} Column {1}", classMapping.DiscriminatorValue, columnName), error);
                }
            }
        }
    }
}