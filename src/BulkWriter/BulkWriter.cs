using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BulkWriter.Internal;
using BulkWriter.Properties;

namespace BulkWriter
{
    public class BulkWriter<TResult> : IBulkWriter<TResult>
    {
        private readonly SqlBulkCopy _sqlBulkCopy;
        private readonly IEnumerable<PropertyMapping> _propertyMappings;

        public BulkWriter(string connectionString, BulkWriterOptions options = null)
        {
            options = options ?? new BulkWriterOptions();
            _propertyMappings = typeof(TResult).BuildMappings();

            var hasAnyKeys = _propertyMappings.Any(x => x.Destination.IsKey);
            var sqlBulkCopyOptions = (hasAnyKeys ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default) | SqlBulkCopyOptions.TableLock;
            var destinationTableName = typeof(TResult).GetTypeInfo().GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TResult).Name;

            var sqlBulkCopy = new SqlBulkCopy(connectionString, sqlBulkCopyOptions)
            {
                DestinationTableName = destinationTableName,
                EnableStreaming = true,
            };

            ApplyOptions(options, sqlBulkCopy);

            foreach (var propertyMapping in _propertyMappings.Where(propertyMapping => propertyMapping.ShouldMap))
            {
                sqlBulkCopy.ColumnMappings.Add(propertyMapping.ToColumnMapping());
            }

            options.BulkCopySetup(sqlBulkCopy);

            if (sqlBulkCopy.ColumnMappings == null /* dispose of SqlBulkCopy will clear this */)
            {
                /* consumer called SqlBulkCopy.Close() in setup callback */
                throw new InvalidOperationException(Resources.BulkWriter_Setup_SqlBulkCopyDisposed);
            }

            _sqlBulkCopy = sqlBulkCopy;
        }

        private static void ApplyOptions(BulkWriterOptions options, SqlBulkCopy sqlBulkCopy)
        {
            sqlBulkCopy.BatchSize = options.BatchSize;
            sqlBulkCopy.BulkCopyTimeout = options.BulkCopyTimeout;
        }

        public void WriteToDatabase(IEnumerable<TResult> items)
        {
            using (var dataReader = new EnumerableDataReader<TResult>(items, _propertyMappings))
            {
                _sqlBulkCopy.WriteToServer(dataReader);
            }
        }

        public async Task WriteToDatabaseAsync(IEnumerable<TResult> items)
        {
            using (var dataReader = new EnumerableDataReader<TResult>(items, _propertyMappings))
            {
                await _sqlBulkCopy.WriteToServerAsync(dataReader);
            }
        }

        public void Dispose() => ((IDisposable)_sqlBulkCopy).Dispose();
    }
}