using System;
using System.Collections.Generic;

namespace Headspring.BulkWriter
{
    public interface IBulkWriter<TResult> : IDisposable
    {
        void WriteToDatabase(IEnumerable<TResult> items);
    }
}