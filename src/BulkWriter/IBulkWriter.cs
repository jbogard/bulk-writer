using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulkWriter
{
    public interface IBulkWriter<in TResult> : IDisposable
    {
        /// <summary>
        /// Bulk loads an input enumerable of type <typeparamref name="TResult"/>
        /// </summary>
        /// <param name="items">Items to load to the database</param>
        void WriteToDatabase(IEnumerable<TResult> items);
        
        /// <summary>
        /// Bulk loads an input enumerable of type <typeparamref name="TResult"/>
        /// </summary>
        /// <param name="items">Items to load to the database</param>
        /// <returns>Awaitable task for writing to the database</returns>
        Task WriteToDatabaseAsync(IEnumerable<TResult> items);
    }
}