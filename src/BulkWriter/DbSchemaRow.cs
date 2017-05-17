using System;
using System.Data;
using System.Globalization;

namespace BulkWriter
{
    public sealed class DbSchemaRow
    {
        private readonly DataRow dataRow;
        private readonly DbSchemaTable schemaTable;

        public DbSchemaRow(DbSchemaTable schemaTable, DataRow dataRow)
        {
            this.schemaTable = schemaTable;
            this.dataRow = dataRow;
        }

        public DataRow DataRow
        {
            get { return this.dataRow; }
        }

        public string ColumnName
        {
            get
            {
                object value = this.dataRow[this.schemaTable.ColumnName, DataRowVersion.Default];
                return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
            }
        }

        public int ColumnOrdinal
        {
            get
            {
                object value = this.dataRow[this.schemaTable.ColumnOrdinal, DataRowVersion.Default];
                return !Convert.IsDBNull(value) ? Convert.ToInt32(value, CultureInfo.InvariantCulture) : -1;
            }
        }

        public int Size
        {
            get
            {
                object value = this.dataRow[this.schemaTable.Size, DataRowVersion.Default];
                return !Convert.IsDBNull(value) ? Convert.ToInt32(value, CultureInfo.InvariantCulture) : 0;
            }
        }

        public string BaseColumnName
        {
            get
            {
                if (this.schemaTable.BaseColumnName != null)
                {
                    object value = this.dataRow[this.schemaTable.BaseColumnName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseServerName
        {
            get
            {
                if (this.schemaTable.BaseServerName != null)
                {
                    object value = this.dataRow[this.schemaTable.BaseServerName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseCatalogName
        {
            get
            {
                if (this.schemaTable.BaseCatalogName != null)
                {
                    object value = this.dataRow[this.schemaTable.BaseCatalogName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseSchemaName
        {
            get
            {
                if (this.schemaTable.BaseSchemaName != null)
                {
                    object value = this.dataRow[this.schemaTable.BaseSchemaName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string BaseTableName
        {
            get
            {
                if (this.schemaTable.BaseTableName != null)
                {
                    object value = this.dataRow[this.schemaTable.BaseTableName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }
                return string.Empty;
            }
        }

        public bool IsAutoIncrement
        {
            get
            {
                if (this.schemaTable.IsAutoIncrement != null)
                {
                    object value = this.dataRow[this.schemaTable.IsAutoIncrement, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsUnique
        {
            get
            {
                if (this.schemaTable.IsUnique != null)
                {
                    object value = this.dataRow[this.schemaTable.IsUnique, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsRowVersion
        {
            get
            {
                if (this.schemaTable.IsRowVersion != null)
                {
                    object value = this.dataRow[this.schemaTable.IsRowVersion, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsKey
        {
            get
            {
                if (this.schemaTable.IsKey != null)
                {
                    object value = this.dataRow[this.schemaTable.IsKey, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsExpression
        {
            get
            {
                if (this.schemaTable.IsExpression != null)
                {
                    object value = this.dataRow[this.schemaTable.IsExpression, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsHidden
        {
            get
            {
                if (this.schemaTable.IsHidden != null)
                {
                    object value = this.dataRow[this.schemaTable.IsHidden, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsLong
        {
            get
            {
                if (this.schemaTable.IsLong != null)
                {
                    object value = this.dataRow[this.schemaTable.IsLong, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this.schemaTable.IsReadOnly != null)
                {
                    object value = this.dataRow[this.schemaTable.IsReadOnly, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        public Type DataType
        {
            get
            {
                if (this.schemaTable.DataType != null)
                {
                    object value = this.dataRow[this.schemaTable.DataType, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? (Type)value : null;
                }
                return null;
            }
        }

        public string DataTypeName
        {
            get
            {
                if (this.schemaTable.DataTypeName != null)
                {
                    object value = this.dataRow[this.schemaTable.DataTypeName, DataRowVersion.Default];
                    return !Convert.IsDBNull(value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : string.Empty;
                }

                return string.Empty;
            }
        }

        public bool AllowDbNull
        {
            get
            {
                if (this.schemaTable.AllowDbNull != null)
                {
                    object value = this.dataRow[this.schemaTable.AllowDbNull, DataRowVersion.Default];
                    return Convert.IsDBNull(value) || Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                return true;
            }
        }

        public int UnsortedIndex
        {
            get { return (int)this.dataRow[this.schemaTable.UnsortedIndex, DataRowVersion.Default]; }
        }
    }

}