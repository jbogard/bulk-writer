using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Headspring.BulkWriter.Nhibernate
{
    public class WrappedBulkCopy : IBulkCopy
    {
        private readonly SqlConnection connection;
        private readonly SqlBulkCopy sqlBulkCopy;
        private readonly SqlTransaction transaction;

        public WrappedBulkCopy(SqlConnection connection, SqlTransaction transaction, SqlBulkCopy sqlBulkCopy)
        {
            this.connection = connection;
            this.transaction = transaction;
            this.sqlBulkCopy = sqlBulkCopy;
        }

        public void Dispose()
        {
            ((IDisposable) this.sqlBulkCopy).Dispose();
            this.transaction.Dispose();
            this.connection.Dispose();
        }

        public void WriteToServer(IDataReader dataReader)
        {
            try
            {
                this.sqlBulkCopy.WriteToServer(dataReader);
                this.transaction.Commit();
            }
            catch (Exception)
            {
                this.transaction.Rollback();
                throw;
            }
        }

        public async Task WriteToServerAsync(IDataReader dataReader)
        {
            try
            {
                await this.sqlBulkCopy.WriteToServerAsync(dataReader);
                this.transaction.Commit();
            }
            catch (Exception)
            {
                this.transaction.Rollback();
                throw;
            }
        }
    }
}