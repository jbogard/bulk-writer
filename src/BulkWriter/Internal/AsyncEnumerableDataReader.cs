#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using BulkWriter.Properties;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("BulkWriter.Tests")]
namespace BulkWriter.Internal
{
    internal class AsyncEnumerableDataReader<TResult> : EnumerableDataReaderBase<TResult>
    {
        private readonly IAsyncEnumerable<TResult> _items;

        private bool _disposed;
        private IAsyncEnumerator<TResult> _enumerator;

        public AsyncEnumerableDataReader(IAsyncEnumerable<TResult> items, IEnumerable<PropertyMapping> propertyMappings)
            : base(propertyMappings)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public override TResult Current
        {
            get
            {
                EnsureNotDisposed();
                return null != _enumerator ? _enumerator.Current : default(TResult);
            }
        }

        public override bool Read() => throw new NotImplementedException();

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            _enumerator ??= _items.GetAsyncEnumerator(cancellationToken);

            return _enumerator.MoveNextAsync().AsTask();
        }


        public override async ValueTask DisposeAsync()
        {
            if (_enumerator != null)
            {
                await _enumerator.DisposeAsync();
                _enumerator = null;
            }
            _disposed = true;
            await base.DisposeAsync();
        }

        protected override void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("AsyncEnumerableDataReader");
            }
        }
    }
}
#endif
