using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
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
        private readonly BulkCopyFactoryOptions options;

        public BulkCopyFactory(Configuration configuration) : this(configuration, BulkCopyFactoryOptions.Default)
        {
        }

        public BulkCopyFactory(Configuration configuration, BulkCopyFactoryOptions options)
        {
            if (null == configuration)
            {
                throw new ArgumentNullException("configuration");
            }
            
            this.configuration = configuration;
            this.options = options;
        }

        public IBulkCopy Create(object item, out IPropertyToOrdinalMappings mappings)
        {
            string connectionString;
            var connection = this.CreateConnectionFromConfiguration(out connectionString);

            TryButDisposeOnFail(connection.Open, connection);

            SqlTransaction transaction = null;
            TryButDisposeOnFail(() =>
            {
                transaction = this.CreateTransactionFromConfiguationOrOptions(connection);
            }, transaction, connection);

            SqlBulkCopy sqlBulkCopy = null;
            TryButDisposeOnFail(() =>
            {
                sqlBulkCopy = this.CreateBulkCopyFromOptions(connection, transaction);
            }, sqlBulkCopy, transaction, connection);

            PropertyToOrdinalMappings mappingsImpl = null;
            WrappedBulkCopy bulkCopy = null;
            TryButDisposeOnFail(() =>
            {
                var type = NHibernateUtil.GetClass(item);
                var tempMappings = new PropertyToOrdinalMappings(type); // So we don't end up with a partially constructed mapping object

                AddColumnMappings(type, connection.ConnectionString, sqlBulkCopy, tempMappings, true);

                var baseType = type.BaseType;
                while (typeof(object) != baseType)
                {
                    AddColumnMappings(baseType, connectionString, sqlBulkCopy, tempMappings, false);
                    Debug.Assert(baseType != null, "baseType != null");
                    baseType = baseType.BaseType;
                }

                mappingsImpl = tempMappings;
                bulkCopy = new WrappedBulkCopy(connection, transaction, sqlBulkCopy);
            }, sqlBulkCopy, transaction, connection);

            mappings = mappingsImpl;

            // We don't dispose our SqlConnection, SqlTransaction or SqlBulkCopy instances before we
            // leave the method because WrappedBulkCopy will take care of their disposal once it is
            // disposed by the caller.

            return bulkCopy;
        }

        private SqlConnection CreateConnectionFromConfiguration(out string connectionString)
        {
            connectionString = this.configuration.GetProperty(Environment.ConnectionString);

            if (null == connectionString)
            {
                string connectionStringName = this.configuration.GetProperty(Environment.ConnectionStringName);
                if (null == connectionStringName)
                {
                    throw new InvalidOperationException("BulkCopyFactory is not able to create a SqlConnection because NHibernate is not configured completely. Please ensure that NHibernate's \"connection.connection_string\" or \"connection.connection_string_name\" properties are set before using this factory to create a IBulkCopy instance.");
                }

                var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
                if (null == connectionStringSettings)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "BulkCopyFactory is not able to create a SqlConnection. NHibernate's \"connection.connection_string_name\" property value is '{0}' but a connection string by that name cannot be found in the application's configuration file.", connectionStringName));
                }

                connectionString = connectionStringSettings.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "BulkCopyFactory is not able to create a SqlConnection. The connection string '{0}' in the application's configuration file cannot be a null or empty string.", connectionStringName));
                }
            }

            var connection = new SqlConnection(connectionString);
            return connection;
        }

        private SqlTransaction CreateTransactionFromConfiguationOrOptions(SqlConnection connection)
        {
            IsolationLevel isolation;

            string isolationLevelSetting = this.configuration.GetProperty(Environment.Isolation);
            if (!Enum.TryParse(isolationLevelSetting, true, out isolation))
            {
                isolation = this.options.IsolationLevel;
            }

            var transaction = connection.BeginTransaction(isolation);
            return transaction;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Instance disposal is managed further up the return stack.")]
        private SqlBulkCopy CreateBulkCopyFromOptions(SqlConnection connection, SqlTransaction transaction)
        {
            var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                BatchSize = this.options.BatchSize,
                BulkCopyTimeout = this.options.Timeout
            };

            return bulkCopy;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method does not map a column if an error occurs.")]
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

            var sortedSchemaRows = SchemaReader.GetSortedSchemaRows(connectionString, tableName);

            if (setDestinationTableName)
            {
                MapDisriminator(classMapping, sortedSchemaRows, mappings, sqlBulkCopy);
            }

            MapProperties(classMapping, sortedSchemaRows, mappings, sqlBulkCopy);
        }

        private static void MapDisriminator(PersistentClass classMapping, IEnumerable<DbSchemaRow> sortedSchemaRows, PropertyToOrdinalMappings mappings, SqlBulkCopy sqlBulkCopy)
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
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to map the Class {0} Column {1}", classMapping.DiscriminatorValue, columnName), error);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignoring exceptions on disposal so we can throw the original exception.")]
        private static void TryButDisposeOnFail(Action action, params IDisposable[] disposables)
        {
            try
            {
                action();
            }
            catch
            {
                if (null != disposables)
                {
                    // the caller should provide the correct disposal order!

                    foreach (var disposable in disposables)
                    {
                        if (null != disposable)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                throw;
            }
        }
    }
}