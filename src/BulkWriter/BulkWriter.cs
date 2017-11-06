using System;
using System.Collections.Generic;
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
            _sqlBulkCopy = Initialize(options => new SqlBulkCopy(connectionString, options));
        }

        public BulkWriter(SqlConnection connection, SqlTransaction transaction = null)
        {
            _propertyMappings = typeof(TResult).BuildMappings();
            _sqlBulkCopy = Initialize(options => new SqlBulkCopy(connection, options, transaction));
        }

        private SqlBulkCopy Initialize(Func<SqlBulkCopyOptions, SqlBulkCopy> createBulkCopy)
        {
            var hasAnyKeys = _propertyMappings.Any(x => x.Destination.IsKey);
            var sqlBulkCopyOptions = (hasAnyKeys ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default)
                | SqlBulkCopyOptions.TableLock;
            var destinationTableName = typeof(TResult).GetTypeInfo().GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TResult).Name;

            var sqlBulkCopy = createBulkCopy(sqlBulkCopyOptions);
            sqlBulkCopy.DestinationTableName = destinationTableName;
            sqlBulkCopy.EnableStreaming = true;
            sqlBulkCopy.BulkCopyTimeout = 0;

            if (BatchSize.HasValue)
                sqlBulkCopy.BatchSize = BatchSize.Value;
            if (BulkCopyTimeout.HasValue)
                sqlBulkCopy.BulkCopyTimeout = BulkCopyTimeout.Value;

            //sqlBulkCopy.BatchSize = BatchSize ?? sqlBulkCopy.BatchSize;
            //sqlBulkCopy.BulkCopyTimeout = BulkCopyTimeout ?? sqlBulkCopy.BulkCopyTimeout;
            BulkCopySetup(sqlBulkCopy);

            foreach (var propertyMapping in _propertyMappings.Where(propertyMapping => propertyMapping.ShouldMap))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(propertyMapping.Source.Ordinal, propertyMapping.Destination.ColumnOrdinal));
            }

            return sqlBulkCopy;
        }

        public int? BatchSize { get; set; }
        public int? BulkCopyTimeout { get; set; }
        public Action<SqlBulkCopy> BulkCopySetup { get; set; } = sbc => {};
 
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