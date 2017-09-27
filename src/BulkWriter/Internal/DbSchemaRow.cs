using System;
using System.Data;
using System.Globalization;

namespace BulkWriter.Internal
{
    public sealed class DbSchemaRow
    {
        private readonly DbSchemaTable _schemaTable;

        public DbSchemaRow(DbSchemaTable schemaTable, DataRow dataRow)
        {
            _schemaTable = schemaTable;
            DataRow = dataRow;
        }

        public DataRow DataRow { get; }

        public string ColumnName
        {
            get
            {
                var value = DataRow[_schemaTable.ColumnName, DataRowVersion.Default];
                return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
            }
        }

        public int ColumnOrdinal
        {
            get
            {
                var value = DataRow[_schemaTable.ColumnOrdinal, DataRowVersion.Default];
                return !Convert.IsDBNull(value) ? Convert.ToInt32(value, CultureInfo.InvariantCulture) : -1;
            }
        }

        public int Size
        {
            get
            {
                var value = DataRow[_schemaTable.Size, DataRowVersion.Default];
                return !Convert.IsDBNull(value) ? Convert.ToInt32(value, CultureInfo.InvariantCulture) : 0;
            }
        }

        public string BaseColumnName
        {
            get
            {
                if (_schemaTable.BaseColumnName != null)
                {
                    var value = DataRow[_schemaTable.BaseColumnName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseServerName
        {
            get
            {
                if (_schemaTable.BaseServerName != null)
                {
                    var value = DataRow[_schemaTable.BaseServerName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseCatalogName
        {
            get
            {
                if (_schemaTable.BaseCatalogName != null)
                {
                    var value = DataRow[_schemaTable.BaseCatalogName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseSchemaName
        {
            get
            {
                if (_schemaTable.BaseSchemaName != null)
                {
                    var value = DataRow[_schemaTable.BaseSchemaName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseTableName
        {
            get
            {
                if (_schemaTable.BaseTableName != null)
                {
                    var value = DataRow[_schemaTable.BaseTableName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public bool IsAutoIncrement
        {
            get
            {
                if (_schemaTable.IsAutoIncrement != null)
                {
                    var value = DataRow[_schemaTable.IsAutoIncrement, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsUnique
        {
            get
            {
                if (_schemaTable.IsUnique != null)
                {
                    var value = DataRow[_schemaTable.IsUnique, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsRowVersion
        {
            get
            {
                if (_schemaTable.IsRowVersion != null)
                {
                    var value = DataRow[_schemaTable.IsRowVersion, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsKey
        {
            get
            {
                if (_schemaTable.IsKey != null)
                {
                    var value = DataRow[_schemaTable.IsKey, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsExpression
        {
            get
            {
                if (_schemaTable.IsExpression != null)
                {
                    var value = DataRow[_schemaTable.IsExpression, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsHidden
        {
            get
            {
                if (_schemaTable.IsHidden != null)
                {
                    var value = DataRow[_schemaTable.IsHidden, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsLong
        {
            get
            {
                if (_schemaTable.IsLong != null)
                {
                    var value = DataRow[_schemaTable.IsLong, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (_schemaTable.IsReadOnly != null)
                {
                    var value = DataRow[_schemaTable.IsReadOnly, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public Type DataType
        {
            get
            {
                if (_schemaTable.DataType != null)
                {
                    var value = DataRow[_schemaTable.DataType, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? (Type)value : null;
                }
                return null;
            }
        }

        public string DataTypeName
        {
            get
            {
                if (_schemaTable.DataTypeName != null)
                {
                    var value = DataRow[_schemaTable.DataTypeName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }

                return string.Empty;
            }
        }

        public bool AllowDbNull
        {
            get
            {
                if (_schemaTable.AllowDbNull != null)
                {
                    var value = DataRow[_schemaTable.AllowDbNull, DataRowVersion.Default];
                    return Convert.IsDBNull(value) || Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return true;
            }
        }

        public int UnsortedIndex => (int)DataRow[_schemaTable.UnsortedIndex, DataRowVersion.Default];
    }

}