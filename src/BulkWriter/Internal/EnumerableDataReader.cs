using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class EnumerableDataReader<TResult> : IDataReader
    {
        private readonly IEnumerable<TResult> _items;
        private readonly Dictionary<string, int> _nameToOrdinalMappings;
        private readonly Dictionary<int, PropertyMapping> _ordinalToPropertyMappings;
        private readonly PropertyMapping[] _propertyMappings;

        private bool _disposed;
        private IEnumerator<TResult> _enumerator;

        public EnumerableDataReader(IEnumerable<TResult> items, IEnumerable<PropertyMapping> propertyMappings)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _propertyMappings = propertyMappings?.OrderBy(x => x.Source.Ordinal).ToArray() ?? throw new ArgumentNullException(nameof(propertyMappings));

            _ordinalToPropertyMappings = _propertyMappings.ToDictionary(x => x.Source.Ordinal);
            _nameToOrdinalMappings = _propertyMappings.ToDictionary(x => x.Source.Property.Name, x => x.Source.Ordinal);
        }

        public TResult Current
        {
            get
            {
                EnsureNotDisposed();
                return null != _enumerator ? _enumerator.Current : default(TResult);
            }
        }

        public int GetOrdinal(string name)
        {
            EnsureNotDisposed();

            if (!_nameToOrdinalMappings.TryGetValue(name, out int ordinal))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetOrdinal_NameDoesNotMapToOrdinal);
            }

            return ordinal;
        }

        public bool Read()
        {
            EnsureNotDisposed();

            if (null == _enumerator)
            {
                _enumerator = _items.GetEnumerator();
            }

            return _enumerator.MoveNext();
        }

        public bool IsDBNull(int i)
        {
            EnsureNotDisposed();

            var value = GetValue(i);
            return null == value;
        }

        public object GetValue(int i)
        {
            EnsureNotDisposed();

            if (!_ordinalToPropertyMappings.TryGetValue(i, out PropertyMapping mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetValue_OrdinalDoesNotMapToProperty);
            }

            var valueGetter = mapping.Source.Property.GetValueGetter();

            var value = valueGetter(_enumerator.Current);
            return value;
        }

        public string GetName(int i)
        {
            EnsureNotDisposed();

            if (!_ordinalToPropertyMappings.TryGetValue(i, out PropertyMapping mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetName_OrdinalDoesNotMapToName);
            }

            var name = mapping.Source.Property.Name;
            return name;
        }

        public int FieldCount
        {
            get
            {
                EnsureNotDisposed();
                return _propertyMappings.Length;
            }
        }

        public void Dispose()
        {
            if (null != _enumerator)
            {
                _enumerator.Dispose();
                _enumerator = null;
            }

            _disposed = true;
        }

        public void Close()
        {
            Dispose();
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("EnumerableDataReader");
            }
        }

        #region Not used by SqlBulkCopy

        public string GetDataTypeName(int i) => throw new NotSupportedException();

        public Type GetFieldType(int i) => throw new NotSupportedException();

        public int GetValues(object[] values) => throw new NotSupportedException();

        public bool GetBoolean(int i) => throw new NotSupportedException();

        public byte GetByte(int i) => throw new NotSupportedException();

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotSupportedException();

        public char GetChar(int i) => throw new NotSupportedException();

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotSupportedException();

        public Guid GetGuid(int i) => throw new NotSupportedException();

        public short GetInt16(int i) => throw new NotSupportedException();

        public int GetInt32(int i) => throw new NotSupportedException();

        public long GetInt64(int i) => throw new NotSupportedException();

        public float GetFloat(int i) => throw new NotSupportedException();

        public double GetDouble(int i) => throw new NotSupportedException();

        public string GetString(int i) => throw new NotSupportedException();

        public decimal GetDecimal(int i) => throw new NotSupportedException();

        public DateTime GetDateTime(int i) => throw new NotSupportedException();

        public IDataReader GetData(int i) => throw new NotSupportedException();

        object IDataRecord.this[int i] => throw new NotSupportedException();

        object IDataRecord.this[string name] => throw new NotSupportedException();

        public DataTable GetSchemaTable() => throw new NotSupportedException();

        public bool NextResult() => throw new NotSupportedException();

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int Depth => throw new NotSupportedException();

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool IsClosed => throw new NotSupportedException();

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int RecordsAffected => throw new NotSupportedException();

        #endregion
    }
}