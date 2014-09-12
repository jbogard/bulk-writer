using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Headspring.BulkWriter.Properties;

namespace Headspring.BulkWriter
{
    internal class Mapping<TResult> : IMapping<TResult>
    {
        private readonly IEnumerable<PropertyMapping> propertyMappings;

        private string destinationTableName;

        public Mapping(string destinationTableName, IEnumerable<PropertyMapping> propertyMappings)
        {
            if (null == propertyMappings)
            {
                throw new ArgumentNullException("propertyMappings");
            }

            this.destinationTableName = destinationTableName;
            this.propertyMappings = propertyMappings;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing.")]
        public IBulkWriter<TResult> CreateBulkWriter(string connectionString)
        {
            if (null == connectionString)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (0 == connectionString.Length)
            {
                throw new ArgumentException(Resources.Mapping_CreateBulkWriter_InvalidConnectionString, "connectionString");
            }

            this.AutoDiscoverIfNeeded(connectionString);

            bool hasAnyKeys = this.propertyMappings.Any(x => x.Destination.IsPropertySet(MappingProperty.IsKey) && x.Destination.IsKey);
            SqlBulkCopyOptions sqlBulkCopyOptions = hasAnyKeys ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default;
            var sqlBulkCopy = new SqlBulkCopy(connectionString, sqlBulkCopyOptions)
            {
                DestinationTableName = this.destinationTableName
            };

            foreach (PropertyMapping propertyMapping in this.propertyMappings)
            {
                if (propertyMapping.ShouldMap)
                {
                    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(propertyMapping.Source.Ordinal, propertyMapping.Destination.ColumnOrdinal));
                }
            }

            return new BulkWriter<TResult>(sqlBulkCopy, this.propertyMappings);
        }

        private void AutoDiscoverIfNeeded(string connectionString)
        {
            if (this.NeedsAutoDiscovery())
            {
                if (null == this.destinationTableName)
                {
                    this.destinationTableName = AutoDiscover.TableName<TResult>(true);
                }

                AutoDiscover.Mappings(connectionString, this.destinationTableName, this.propertyMappings);
            }
        }

        private bool NeedsAutoDiscovery()
        {
            if (null == this.destinationTableName)
            {
                return true;
            }

            foreach (PropertyMapping propertyMapping in this.propertyMappings)
            {
                for (int i = 0; i < MappingDestination.PropertyIndexCount; i++)
                {
                    if (!propertyMapping.Destination.IsPropertySet(i))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}