using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace BulkWriter.Internal
{
    public static class SchemaReader
    {
        private const string SchemaMappingUnsortedIndex = "SchemaMapping Unsorted Index";

        public static DbSchemaRow[] GetSortedSchemaRows(string connectionString, string quotedTableName)
        {
            DataTable schemaTable;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var selectSql = string.Format(CultureInfo.InvariantCulture, "select * from {0}", quotedTableName);
                using (var command = new SqlCommand(selectSql, connection))
                {
                    using (IDataReader dataReader = command.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
                    {
                        schemaTable = dataReader.GetSchemaTable();
                    }
                }
            }

            var schemaRows = GetSortedSchemaRows(schemaTable, false);
            return schemaRows;
        }

        private static DbSchemaRow[] GetSortedSchemaRows(DataTable dataTable, bool returnProviderSpecificTypes)
        {
            var column = dataTable.Columns[SchemaMappingUnsortedIndex];
            if (column == null)
            {
                column = new DataColumn(SchemaMappingUnsortedIndex, typeof (int));
                dataTable.Columns.Add(column);
            }

            var count = dataTable.Rows.Count;
            for (var index = 0; index < count; ++index)
            {
                dataTable.Rows[index][column] = index;
            }
            
            var schemaTable = new DbSchemaTable(dataTable, returnProviderSpecificTypes);
            var dataRowArray = dataTable.Select(null, "ColumnOrdinal ASC", DataViewRowState.CurrentRows);
            
            var dbSchemaRowArray = new DbSchemaRow[dataRowArray.Length];
            for (var index = 0; index < dataRowArray.Length; ++index)
            {
                dbSchemaRowArray[index] = new DbSchemaRow(schemaTable, dataRowArray[index]);
            }
            
            return dbSchemaRowArray;
        }
    }
}