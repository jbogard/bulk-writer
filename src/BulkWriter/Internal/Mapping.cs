using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class Mapping<TResult> : IMapping<TResult>
    {
        private readonly IEnumerable<PropertyMapping> _propertyMappings;

        private string _destinationTableName;

        public Mapping(string destinationTableName, IEnumerable<PropertyMapping> propertyMappings)
        {
            _destinationTableName = destinationTableName;
            _propertyMappings = propertyMappings ?? throw new ArgumentNullException(nameof(propertyMappings));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing.")]
        public IBulkWriter<TResult> CreateBulkWriter(string connectionString)
        {
            if (null == connectionString)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (0 == connectionString.Length)
            {
                throw new ArgumentException(Resources.Mapping_CreateBulkWriter_InvalidConnectionString, nameof(connectionString));
            }

            AutoDiscoverIfNeeded(connectionString);

            var hasAnyKeys = _propertyMappings.Any(x => x.Destination.IsPropertySet(MappingProperty.IsKey) && x.Destination.IsKey);
            var sqlBulkCopyOptions = hasAnyKeys ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default;
            var sqlBulkCopy = new SqlBulkCopy(connectionString, sqlBulkCopyOptions)
            {
                DestinationTableName = _destinationTableName
            };

            foreach (var propertyMapping in _propertyMappings.Where(propertyMapping => propertyMapping.ShouldMap))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(propertyMapping.Source.Ordinal, propertyMapping.Destination.ColumnOrdinal));
            }

            return new BulkWriter<TResult>(sqlBulkCopy, _propertyMappings);
        }

        private void AutoDiscoverIfNeeded(string connectionString)
        {
            if (!NeedsAutoDiscovery()) return;
            if (null == _destinationTableName)
            {
                _destinationTableName = AutoDiscover.TableName<TResult>(true);
            }

            AutoDiscover.Mappings(connectionString, _destinationTableName, _propertyMappings);
        }

        private bool NeedsAutoDiscovery()
        {
            if (null == _destinationTableName)
            {
                return true;
            }

            foreach (var propertyMapping in _propertyMappings)
            {
                for (var i = 0; i < MappingDestination.PropertyIndexCount; i++)
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