using System.Data;
using System.Data.Common;

namespace BulkWriter.Internal
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
            "DataTypeName",
            "SchemaMapping Unsorted Index"
        };

        private readonly DataColumn[] _columnCache = new DataColumn[DbcolumnName.Length];
        private readonly DataColumnCollection _columns;
        private readonly bool _returnProviderSpecificTypes;

        public DbSchemaTable(DataTable dataTable, bool returnProviderSpecificTypes)
        {
            _columns = dataTable.Columns;
            _returnProviderSpecificTypes = returnProviderSpecificTypes;
        }

        public DataColumn ColumnName => CachedDataColumn(ColumnEnum.ColumnName);

        public DataColumn Size => CachedDataColumn(ColumnEnum.ColumnSize);

        public DataColumn BaseServerName => CachedDataColumn(ColumnEnum.BaseServerName);

        public DataColumn BaseColumnName => CachedDataColumn(ColumnEnum.BaseColumnName);

        public DataColumn BaseTableName => CachedDataColumn(ColumnEnum.BaseTableName);

        public DataColumn BaseCatalogName => CachedDataColumn(ColumnEnum.BaseCatalogName);

        public DataColumn BaseSchemaName => CachedDataColumn(ColumnEnum.BaseSchemaName);

        public DataColumn IsAutoIncrement => CachedDataColumn(ColumnEnum.IsAutoIncrement);

        public DataColumn IsUnique => CachedDataColumn(ColumnEnum.IsUnique);

        public DataColumn IsKey => CachedDataColumn(ColumnEnum.IsKey);

        public DataColumn IsRowVersion => CachedDataColumn(ColumnEnum.IsRowVersion);

        public DataColumn AllowDbNull => CachedDataColumn(ColumnEnum.AllowDbNull);

        public DataColumn IsExpression => CachedDataColumn(ColumnEnum.IsExpression);

        public DataColumn IsHidden => CachedDataColumn(ColumnEnum.IsHidden);

        public DataColumn IsLong => CachedDataColumn(ColumnEnum.IsLong);

        public DataColumn IsReadOnly => CachedDataColumn(ColumnEnum.IsReadOnly);

        public DataColumn UnsortedIndex => CachedDataColumn(ColumnEnum.SchemaMappingUnsortedIndex);

        public DataColumn ColumnOrdinal => CachedDataColumn(ColumnEnum.ColumnOrdinal);

        public DataColumn DataType
        {
            get
            {
                if (_returnProviderSpecificTypes)
                {
                    return CachedDataColumn(ColumnEnum.ProviderSpecificDataType, ColumnEnum.DataType);
                }
                return CachedDataColumn(ColumnEnum.DataType);
            }
        }

        public DataColumn DataTypeName => CachedDataColumn(ColumnEnum.DataTypeName);

        private DataColumn CachedDataColumn(ColumnEnum column) => CachedDataColumn(column, column);

        private DataColumn CachedDataColumn(ColumnEnum column, ColumnEnum column2)
        {
            var dataColumn = _columnCache[(int)column];
            if (dataColumn == null)
            {
                var index = _columns.IndexOf(DbcolumnName[(int)column]);
                if (-1 == index && column != column2)
                {
                    index = _columns.IndexOf(DbcolumnName[(int)column2]);
                }
                if (-1 != index)
                {
                    dataColumn = _columns[index];
                    _columnCache[(int)column] = dataColumn;
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
            AllowDbNull,
            ProviderType,
            IsExpression,
            IsHidden,
            IsLong,
            IsReadOnly,
            DataTypeName,
            SchemaMappingUnsortedIndex
        }
    }
}