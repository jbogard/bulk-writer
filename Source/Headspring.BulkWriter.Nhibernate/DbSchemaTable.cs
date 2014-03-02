using System.Data;
using System.Data.Common;

namespace Headspring.BulkWriter.Nhibernate
{
    public sealed class DbSchemaTable
    {
        private static readonly string[] DbcolumnName =
        {
            SchemaTableColumn.ColumnName,
            SchemaTableColumn.ColumnOrdinal,
            SchemaTableColumn.ColumnSize,
            SchemaTableOptionalColumn.BaseServerName,
            SchemaTableOptionalColumn.BaseCatalogName,
            SchemaTableColumn.BaseColumnName,
            SchemaTableColumn.BaseSchemaName,
            SchemaTableColumn.BaseTableName,
            SchemaTableOptionalColumn.IsAutoIncrement,
            SchemaTableColumn.IsUnique,
            SchemaTableColumn.IsKey,
            SchemaTableOptionalColumn.IsRowVersion,
            SchemaTableColumn.DataType,
            SchemaTableOptionalColumn.ProviderSpecificDataType,
            SchemaTableColumn.AllowDBNull,
            SchemaTableColumn.ProviderType,
            SchemaTableColumn.IsExpression,
            SchemaTableOptionalColumn.IsHidden,
            SchemaTableColumn.IsLong,
            SchemaTableOptionalColumn.IsReadOnly,
            "SchemaMapping Unsorted Index"
        };

        private readonly DataColumn[] columnCache = new DataColumn[DbcolumnName.Length];
        private readonly DataColumnCollection columns;
        private readonly bool returnProviderSpecificTypes;

        public DbSchemaTable(DataTable dataTable, bool returnProviderSpecificTypes)
        {
            this.columns = dataTable.Columns;
            this.returnProviderSpecificTypes = returnProviderSpecificTypes;
        }

        public DataColumn ColumnName
        {
            get { return this.CachedDataColumn(ColumnEnum.ColumnName); }
        }

        public DataColumn Size
        {
            get { return this.CachedDataColumn(ColumnEnum.ColumnSize); }
        }

        public DataColumn BaseServerName
        {
            get { return this.CachedDataColumn(ColumnEnum.BaseServerName); }
        }

        public DataColumn BaseColumnName
        {
            get { return this.CachedDataColumn(ColumnEnum.BaseColumnName); }
        }

        public DataColumn BaseTableName
        {
            get { return this.CachedDataColumn(ColumnEnum.BaseTableName); }
        }

        public DataColumn BaseCatalogName
        {
            get { return this.CachedDataColumn(ColumnEnum.BaseCatalogName); }
        }

        public DataColumn BaseSchemaName
        {
            get { return this.CachedDataColumn(ColumnEnum.BaseSchemaName); }
        }

        public DataColumn IsAutoIncrement
        {
            get { return this.CachedDataColumn(ColumnEnum.IsAutoIncrement); }
        }

        public DataColumn IsUnique
        {
            get { return this.CachedDataColumn(ColumnEnum.IsUnique); }
        }

        public DataColumn IsKey
        {
            get { return this.CachedDataColumn(ColumnEnum.IsKey); }
        }

        public DataColumn IsRowVersion
        {
            get { return this.CachedDataColumn(ColumnEnum.IsRowVersion); }
        }

        public DataColumn AllowDbNull
        {
            get { return this.CachedDataColumn(ColumnEnum.AllowDBNull); }
        }

        public DataColumn IsExpression
        {
            get { return this.CachedDataColumn(ColumnEnum.IsExpression); }
        }

        public DataColumn IsHidden
        {
            get { return this.CachedDataColumn(ColumnEnum.IsHidden); }
        }

        public DataColumn IsLong
        {
            get { return this.CachedDataColumn(ColumnEnum.IsLong); }
        }

        public DataColumn IsReadOnly
        {
            get { return this.CachedDataColumn(ColumnEnum.IsReadOnly); }
        }

        public DataColumn UnsortedIndex
        {
            get { return this.CachedDataColumn(ColumnEnum.SchemaMappingUnsortedIndex); }
        }

        public DataColumn ColumnOrdinal
        {
            get { return this.CachedDataColumn(ColumnEnum.ColumnOrdinal); }
        }

        public DataColumn DataType
        {
            get
            {
                if (this.returnProviderSpecificTypes)
                {
                    return this.CachedDataColumn(ColumnEnum.ProviderSpecificDataType, ColumnEnum.DataType);
                }
                return this.CachedDataColumn(ColumnEnum.DataType);
            }
        }

        private DataColumn CachedDataColumn(ColumnEnum column)
        {
            return this.CachedDataColumn(column, column);
        }

        private DataColumn CachedDataColumn(ColumnEnum column, ColumnEnum column2)
        {
            DataColumn dataColumn = this.columnCache[(int) column];
            if (dataColumn == null)
            {
                int index = this.columns.IndexOf(DbcolumnName[(int) column]);
                if (-1 == index && column != column2)
                {
                    index = this.columns.IndexOf(DbcolumnName[(int) column2]);
                }
                if (-1 != index)
                {
                    dataColumn = this.columns[index];
                    this.columnCache[(int) column] = dataColumn;
                }
            }
            return dataColumn;
        }

        private enum ColumnEnum
        {
            ColumnName,
            ColumnOrdinal,
            ColumnSize,
            BaseServerName,
            BaseCatalogName,
            BaseColumnName,
            BaseSchemaName,
            BaseTableName,
            IsAutoIncrement,
            IsUnique,
            IsKey,
            IsRowVersion,
            DataType,
            ProviderSpecificDataType,
            AllowDBNull,
            ProviderType,
            IsExpression,
            IsHidden,
            IsLong,
            IsReadOnly,
            SchemaMappingUnsortedIndex,
        }
    }
}