using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Headspring.BulkWriter
{
    internal sealed class BulkWriter<TResult> : IBulkWriter<TResult>
    {
        private readonly SqlBulkCopy sqlBulkCopy;
        private readonly IEnumerable<PropertyMapping> propertyMappings;

        public BulkWriter(SqlBulkCopy sqlBulkCopy, IEnumerable<PropertyMapping> propertyMappings)
        {
            if (null == sqlBulkCopy)
            {
                throw new ArgumentNullException("sqlBulkCopy");
            }

            if (null == propertyMappings)
            {
                throw new ArgumentNullException("propertyMappings");
            }

            this.sqlBulkCopy = sqlBulkCopy;
            this.propertyMappings = propertyMappings;
        }

        public void WriteToDatabase(IEnumerable<TResult> items)
        {
            using (var dataReader = new EnumerableDataReader<TResult>(items, this.propertyMappings))
            {
                this.sqlBulkCopy.WriteToServer(dataReader);
            }
        }

        public void Dispose()
        {
            ((IDisposable) this.sqlBulkCopy).Dispose();
        }
    }
}