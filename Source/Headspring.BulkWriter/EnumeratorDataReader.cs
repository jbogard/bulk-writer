using System;
using System.Collections;
using System.Data;

namespace Headspring.BulkWriter
{
    public sealed class EnumeratorDataReader : IDataReader
    {
        private readonly IEnumerator enumerator;
        private readonly object firstItem;
        private readonly IPropertyToOrdinalMappings mappings;

        private object current;
        private bool firstItemRead;

        public EnumeratorDataReader(object firstItem, IEnumerator enumerator, IPropertyToOrdinalMappings mappings)
        {
            this.firstItem = firstItem;
            this.enumerator = enumerator;
            this.mappings = mappings;
        }

        public int GetOrdinal(string name)
        {
            int ordinal = this.mappings.GetOrdinal(name);
            return ordinal;
        }

        public bool Read()
        {
            if (!this.firstItemRead)
            {
                this.current = this.firstItem;
                this.firstItemRead = true;
                return true;
            }

            bool moveNext = this.enumerator.MoveNext();
            if (moveNext)
            {
                this.current = this.enumerator.Current;
            }

            return moveNext;
        }

        public bool IsDBNull(int i)
        {
            object value = this.mappings.GetValue(i, this.current);
            return (null == value);
        }

        public object GetValue(int i)
        {
            object value = this.mappings.GetValue(i, this.current);
            return value;
        }

        public string GetName(int i)
        {
            string name = this.mappings.GetName(i);
            return name;
        }

        public int FieldCount
        {
            get { return this.mappings.FieldCount; }
        }

        public void Dispose()
        {
            // We have nothing to dispose
        }

        #region Not used by SqlBulkCopy

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}