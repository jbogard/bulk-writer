using System;
using System.Data;
using System.Threading.Tasks;

namespace Headspring.BulkWriter
{
    public interface IBulkCopy : IDisposable
    {
        void WriteToServer(IDataReader dataReader);

        Task WriteToServerAsync(IDataReader dataReader);
    }
}