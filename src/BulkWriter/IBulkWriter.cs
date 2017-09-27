using System;
using System.Collections.Generic;

namespace BulkWriter
{
    public interface IBulkWriter<in TResult> : IDisposable
    {
        void WriteToDatabase(IEnumerable<TResult> items);
    }
}