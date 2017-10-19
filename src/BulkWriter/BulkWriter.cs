using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BulkWriter.Internal;

namespace BulkWriter
{
    public class BulkWriter<TResult> : IBulkWriter<TResult>
    {
        private readonly SqlBulkCopy _sqlBulkCopy;
        private readonly IEnumerable<PropertyMapping> _propertyMappings;

        public BulkWriter(string connectionString)
        {
            _propertyMappings = typeof(TResult).BuildMappings();

            var hasAnyKeys = _propertyMappings.Any(x => x.Destination.IsKey);
            var sqlBulkCopyOptions = (hasAnyKeys ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default)
                | SqlBulkCopyOptions.TableLock;
            var destinationTableName = typeof(TResult).GetTypeInfo().GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TResult).Name;

            var sqlBulkCopy = new SqlBulkCopy(connectionString, sqlBulkCopyOptions)
            {
                DestinationTableName = destinationTableName,
                EnableStreaming = true,
                BulkCopyTimeout = 0
            };

            foreach (var propertyMapping in _propertyMappings.Where(propertyMapping => propertyMapping.ShouldMap))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(propertyMapping.Source.Ordinal, propertyMapping.Destination.ColumnOrdinal));
            }

            _sqlBulkCopy = sqlBulkCopy;
        }

        public int BatchSize
        {
            get => _sqlBulkCopy.BatchSize;
            set => _sqlBulkCopy.BatchSize = value;
        }

        public int BulkCopyTimeout
        {
            get => _sqlBulkCopy.BulkCopyTimeout;
            set => _sqlBulkCopy.BulkCopyTimeout = value;
        }

        public Action<SqlBulkCopy> BulkCopySetup { get; set; } = sbc => { };

        public void WriteToDatabase(IEnumerable<TResult> items)
        {
            BulkCopySetup(_sqlBulkCopy);

            using (var dataReader = new EnumerableDataReader<TResult>(items, _propertyMappings))
            {
                _sqlBulkCopy.WriteToServer(dataReader);
            }
        }

        public async Task WriteToDatabaseAsync(IEnumerable<TResult> items)
        {
            BulkCopySetup(_sqlBulkCopy);

            using (var dataReader = new EnumerableDataReader<TResult>(items, _propertyMappings))
            {
                await _sqlBulkCopy.WriteToServerAsync(dataReader);
            }
        }

        public void Dispose() => ((IDisposable)_sqlBulkCopy).Dispose();
    }
}