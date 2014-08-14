using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Headspring.BulkWriter.DecoratedModel
{
    public sealed class WrappedBulkCopy : IBulkCopy
    {
        private readonly SqlBulkCopy sqlBulkCopy;

        public WrappedBulkCopy(SqlBulkCopy sqlBulkCopy)
        {
            this.sqlBulkCopy = sqlBulkCopy;
        }

        public void Dispose()
        {
            ((IDisposable)this.sqlBulkCopy).Dispose();
        }

        public void WriteToServer(IDataReader dataReader)
        {
            this.sqlBulkCopy.WriteToServer(dataReader);
        }

        public Task WriteToServerAsync(IDataReader dataReader)
        {
            return this.sqlBulkCopy.WriteToServerAsync(dataReader);
        }
    }
}