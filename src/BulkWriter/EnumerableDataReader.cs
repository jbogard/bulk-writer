using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BulkWriter.Internal;
using BulkWriter.Properties;

namespace BulkWriter
{
    public class EnumerableDataReader<TResult> : IDataReader
    {
        private readonly IEnumerable<TResult> items;
        private readonly Dictionary<string, int> nameToOrdinalMappings;
        private readonly Dictionary<int, PropertyMapping> ordinalToPropertyMappings;
        private readonly PropertyMapping[] propertyMappings;

        private bool disposed;
        private IEnumerator<TResult> enumerator;

        public EnumerableDataReader(IEnumerable<TResult> items, IEnumerable<PropertyMapping> propertyMappings)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }

            if (null == propertyMappings)
            {
                throw new ArgumentNullException("propertyMappings");
            }

            this.items = items;
            this.propertyMappings = propertyMappings.OrderBy(x => x.Source.Ordinal).ToArray();

            this.ordinalToPropertyMappings = this.propertyMappings.ToDictionary(x => x.Source.Ordinal);
            this.nameToOrdinalMappings = this.propertyMappings.ToDictionary(x => x.Source.Property.Name, x => x.Source.Ordinal);
        }

        public TResult Current
        {
            get
            {
                this.EnsureNotDisposed();
                return (null != this.enumerator) ? this.enumerator.Current : default(TResult);
            }
        }

        public int GetOrdinal(string name)
        {
            this.EnsureNotDisposed();

            int ordinal;
            if (!this.nameToOrdinalMappings.TryGetValue(name, out ordinal))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetOrdinal_NameDoesNotMapToOrdinal);
            }

            return ordinal;
        }

        public bool Read()
        {
            this.EnsureNotDisposed();

            if (null == this.enumerator)
            {
                this.enumerator = this.items.GetEnumerator();
            }

            return this.enumerator.MoveNext();
        }

        public bool IsDBNull(int i)
        {
            this.EnsureNotDisposed();

            object value = this.GetValue(i);
            return (null == value);
        }

        public object GetValue(int i)
        {
            this.EnsureNotDisposed();

            PropertyMapping mapping;
            if (!this.ordinalToPropertyMappings.TryGetValue(i, out mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetValue_OrdinalDoesNotMapToProperty);
            }

            GetPropertyValueHandler valueGetter = mapping.Source.Property.GetValueGetter();

            object value = valueGetter(this.enumerator.Current);
            return value;
        }

        public string GetName(int i)
        {
            this.EnsureNotDisposed();

            PropertyMapping mapping;
            if (!this.ordinalToPropertyMappings.TryGetValue(i, out mapping))
            {
                throw new InvalidOperationException(Resources.EnumerableDataReader_GetName_OrdinalDoesNotMapToName);
            }

            string name = mapping.Source.Property.Name;
            return name;
        }

        public int FieldCount
        {
            get
            {
                this.EnsureNotDisposed();
                return this.propertyMappings.Length;
            }
        }

        public void Dispose()
        {
            if (null != this.enumerator)
            {
                this.enumerator.Dispose();
                this.enumerator = null;
            }

            this.disposed = true;
        }

        public void Close()
        {
            this.Dispose();
        }

        private void EnsureNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("EnumerableDataReader");
            }
        }

        #region Not used by SqlBulkCopy

        public string GetDataTypeName(int i)
        {
            throw new NotSupportedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotSupportedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotSupportedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotSupportedException();
        }

        public byte GetByte(int i)
        {
            throw new NotSupportedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public char GetChar(int i)
        {
            throw new NotSupportedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotSupportedException();
        }

        public short GetInt16(int i)
        {
            throw new NotSupportedException();
        }

        public int GetInt32(int i)
        {
            throw new NotSupportedException();
        }

        public long GetInt64(int i)
        {
            throw new NotSupportedException();
        }

        public float GetFloat(int i)
        {
            throw new NotSupportedException();
        }

        public double GetDouble(int i)
        {
            throw new NotSupportedException();
        }

        public string GetString(int i)
        {
            throw new NotSupportedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotSupportedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotSupportedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        object IDataRecord.this[int i]
        {
            get { throw new NotSupportedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotSupportedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public bool NextResult()
        {
            throw new NotSupportedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int Depth
        {
            get { throw new NotSupportedException(); }
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool IsClosed
        {
            get { throw new NotSupportedException(); }
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int RecordsAffected
        {
            get { throw new NotSupportedException(); }
        }

        #endregion
    }
}