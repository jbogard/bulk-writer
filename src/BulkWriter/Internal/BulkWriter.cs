using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BulkWriter.Internal
{
    public sealed class BulkWriter<TResult> : IBulkWriter<TResult>
    {
        private readonly SqlBulkCopy _sqlBulkCopy;
        private readonly IEnumerable<PropertyMapping> _propertyMappings;

        public BulkWriter(SqlBulkCopy sqlBulkCopy, IEnumerable<PropertyMapping> propertyMappings)
        {
            _sqlBulkCopy = sqlBulkCopy ?? throw new ArgumentNullException(nameof(sqlBulkCopy));
            _propertyMappings = propertyMappings ?? throw new ArgumentNullException(nameof(propertyMappings));
        }

        public void WriteToDatabase(IEnumerable<TResult> items)
        {
            using (var dataReader = new EnumerableDataReader<TResult>(items, _propertyMappings))
            {
                _sqlBulkCopy.WriteToServer(dataReader);
            }
        }

        public void Dispose() => ((IDisposable)_sqlBulkCopy).Dispose();
    }
}