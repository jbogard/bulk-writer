using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using BulkWriter.Properties;

[assembly: InternalsVisibleTo("BulkWriter.Tests")]
namespace BulkWriter.Internal
{
    internal abstract class EnumerableDataReaderBase<TResult> : DbDataReader
    {
        private readonly PropertyMapping[] _propertyMappings;
        private readonly Dictionary<string, int> _nameToOrdinalMappings;
        private readonly Dictionary<int, PropertyMapping> _ordinalToPropertyMappings;

        protected EnumerableDataReaderBase(IEnumerable<PropertyMapping> propertyMappings)
        {
            _propertyMappings = propertyMappings?.OrderBy(x => x.Source.Ordinal).ToArray() ?? throw new ArgumentNullException(nameof(propertyMappings));

            // Map the source entity's positional ordinals to the source/destination property mapping.
            _ordinalToPropertyMappings = _propertyMappings.ToDictionary(x => x.Source.Ordinal);

            // Map the destination table's ordinals to the source/destination property mapping,
            //   using the source property's name as the key.
            _nameToOrdinalMappings = _propertyMappings.ToDictionary(x => x.Source.Property.Name, x => x.Destination.ColumnOrdinal);

        }

        public abstract TResult Current { get; }

        protected abstract void EnsureNotDisposed();

        public override int FieldCount
        {
            get
            {
                EnsureNotDisposed();
                return _propertyMappings.Length;
            }
        }

        public override bool HasRows => throw new NotSupportedException();
        public override int Depth => throw new NotSupportedException();
        public override bool IsClosed => throw new NotSupportedException();
        public override int RecordsAffected => throw new NotSupportedException();

        public override int GetOrdinal(string name)
        {
            EnsureNotDisposed();

            if (!_nameToOrdinalMappings.TryGetValue(name, out int ordinal))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetOrdinal_NameDoesNotMapToOrdinal);
            }

            return ordinal;
        }

        public override bool IsDBNull(int i)
        {
            EnsureNotDisposed();

            var value = GetValue(i);
            return null == value;
        }

        public override object GetValue(int i)
        {
            EnsureNotDisposed();

            if (!_ordinalToPropertyMappings.TryGetValue(i, out PropertyMapping mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetValue_OrdinalDoesNotMapToProperty);
            }

            var valueGetter = mapping.Source.Property.GetValueGetter();

            var value = valueGetter(Current);
            return value;
        }

        public override string GetString(int i)
        {
            EnsureNotDisposed();

            if (!_ordinalToPropertyMappings.TryGetValue(i, out PropertyMapping mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetString_OrdinalDoesNotMapToProperty);
            }

            var valueGetter = mapping.Source.Property.GetValueGetter();

            var value = valueGetter(Current);
            return value?.ToString();
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            EnsureNotDisposed();

            if (!_ordinalToPropertyMappings.TryGetValue(i, out PropertyMapping mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetBytes_OrdinalDoesNotMapToProperty);
            }

            var valueGetter = mapping.Source.Property.GetValueGetter();
            if (valueGetter(Current) is byte[] value)
            {
                var pos = Math.Max(fieldOffset, fieldOffset / buffer.Length * buffer.Length);
                var rest = value.Length - pos;
                var count = Math.Min(rest, buffer.Length);
                Buffer.BlockCopy(value, (int)fieldOffset, buffer, bufferoffset, (int)count);
                return count;
            }

            return 0;
        }

        public override string GetName(int i)
        {
            EnsureNotDisposed();

            if (!_ordinalToPropertyMappings.TryGetValue(i, out PropertyMapping mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetName_OrdinalDoesNotMapToName);
            }

            var name = mapping.Source.Property.Name;
            return name;
        }

        public override string GetDataTypeName(int i) => throw new NotSupportedException();
        public override IEnumerator GetEnumerator() => throw new NotImplementedException();
        public override Type GetFieldType(int i) => throw new NotSupportedException();
        public override int GetValues(object[] values) => throw new NotSupportedException();
        public override bool GetBoolean(int i) => throw new NotSupportedException();
        public override byte GetByte(int i) => throw new NotSupportedException();
        public override char GetChar(int i) => throw new NotSupportedException();
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public override Guid GetGuid(int i) => throw new NotSupportedException();
        public override short GetInt16(int i) => throw new NotSupportedException();
        public override int GetInt32(int i) => throw new NotSupportedException();
        public override long GetInt64(int i) => throw new NotSupportedException();
        public override float GetFloat(int i) => throw new NotSupportedException();
        public override double GetDouble(int i) => throw new NotSupportedException();
        public override decimal GetDecimal(int i) => throw new NotSupportedException();
        public override DateTime GetDateTime(int i) => throw new NotSupportedException();
        public override object this[int i] => throw new NotSupportedException();
        public override object this[string name] => throw new NotSupportedException();
        public override bool NextResult() => throw new NotSupportedException();
    }

    internal class EnumerableDataReader<TResult> : EnumerableDataReaderBase<TResult>
    {
        private readonly IEnumerable<TResult> _items;

        private bool _disposed;
        private IEnumerator<TResult> _enumerator;

        public EnumerableDataReader(IEnumerable<TResult> items, IEnumerable<PropertyMapping> propertyMappings)
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

        public override bool Read()
        {
            EnsureNotDisposed();

            if (null == _enumerator)
            {
                _enumerator = _items.GetEnumerator();
            }

            return _enumerator.MoveNext();
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (null != _enumerator)
                {
                    _enumerator.Dispose();
                    _enumerator = null;
                }

                _disposed = true;
            }
        }

        protected override void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("EnumerableDataReader");
            }
        }
    }
}
