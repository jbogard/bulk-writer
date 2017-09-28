using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulkWriter
{
    public interface IBulkWriter<in TResult> : IDisposable
    {
        void WriteToDatabase(IEnumerable<TResult> items);
        Task WriteToDatabaseAsync(IEnumerable<TResult> items);
    }
}