using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BulkWriter.Internal;

namespace BulkWriter
{
    /// <summary>
    /// Implements an object that takes an enumerable of <typeparamref name="TResult"/> and writes them to a target database via <c>SqlBulkCopy</c>
    /// </summary>
    /// <typeparam name="TResult">Type of objects to be written to the database</typeparam>
    /// <inheritdoc cref="IBulkWriter{TResult}"/>
    public sealed class BulkWriter<TResult> : IBulkWriter<TResult>
    {
        private readonly SqlBulkCopy _sqlBulkCopy;
        private readonly IEnumerable<PropertyMapping> _propertyMappings;

        public BulkWriter(string connectionString)
        {
            _propertyMappings = typeof(TResult).BuildMappings();
            _sqlBulkCopy = Initialize(options => new SqlBulkCopy(connectionString, options), null);
        }

        public BulkWriter(string connectionString, SqlBulkCopyOptions options)
        {
            _propertyMappings = typeof(TResult).BuildMappings();
            _sqlBulkCopy = Initialize(sbcOpts => new SqlBulkCopy(connectionString, sbcOpts), options);
        }

        public BulkWriter(SqlConnection connection, SqlTransaction transaction = null)
        {
            _propertyMappings = typeof(TResult).BuildMappings();
            _sqlBulkCopy = Initialize(options => new SqlBulkCopy(connection, options, transaction), null);
        }

        public BulkWriter(SqlConnection connection, SqlBulkCopyOptions options, SqlTransaction transaction = null)
        {
            _propertyMappings = typeof(TResult).BuildMappings();
            _sqlBulkCopy = Initialize(sbcOpts => new SqlBulkCopy(connection, sbcOpts, transaction), options);
        }

        private SqlBulkCopy Initialize(Func<SqlBulkCopyOptions, SqlBulkCopy> createBulkCopy, SqlBulkCopyOptions? options)
        {
            SqlBulkCopyOptions sqlBulkCopyOptions;

            if (options == null)
            {
                var hasAnyKeys = _propertyMappings.Any(x => x.Destination.IsKey);
                sqlBulkCopyOptions = (hasAnyKeys ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default)
                                     | SqlBulkCopyOptions.TableLock;
            }
            else
            {
                sqlBulkCopyOptions = options.Value;
            }

            var tableAttribute = typeof(TResult).GetTypeInfo().GetCustomAttribute<TableAttribute>();
            var schemaName = tableAttribute?.Schema;
            var tableName = tableAttribute?.Name ?? typeof(TResult).Name;
            var destinationTableName = schemaName != null ? $"{schemaName}.{tableName}" : tableName;

            var sqlBulkCopy = createBulkCopy(sqlBulkCopyOptions);

            sqlBulkCopy.DestinationTableName = destinationTableName;
            sqlBulkCopy.EnableStreaming = true;
            sqlBulkCopy.BulkCopyTimeout = 0;

            foreach (var propertyMapping in _propertyMappings.Where(propertyMapping => propertyMapping.ShouldMap))
            {
                sqlBulkCopy.ColumnMappings.Add(propertyMapping.ToColumnMapping());
            }

            return sqlBulkCopy;
        }

        /// <summary>
        /// Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server.
        /// </summary>
        public int BatchSize
        {
            get => _sqlBulkCopy.BatchSize;
            set => _sqlBulkCopy.BatchSize = value;
        }

        /// <summary>
        /// Number of seconds for the BulkCopy operation to complete before it times out
        /// </summary>
        public int BulkCopyTimeout
        {
            get => _sqlBulkCopy.BulkCopyTimeout;
            set => _sqlBulkCopy.BulkCopyTimeout = value;
        }

        /// <summary>
        /// Action to allow custom configuration of the <c>SqlBulkCopy</c> object prior to the start of the operation.
        /// </summary>
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