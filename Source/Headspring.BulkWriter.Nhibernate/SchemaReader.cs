using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Headspring.BulkWriter.Nhibernate
{
    internal static class SchemaReader
    {
        private const string SchemaMappingUnsortedIndex = "SchemaMapping Unsorted Index";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "SchemaReader is internal.")]
        public static DbSchemaRow[] GetSortedSchemaRows(string connectionString, string quotedTableName)
        {
            DataTable schemaTable;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectSql = string.Format(CultureInfo.InvariantCulture, "select * from {0}", quotedTableName);
                using (var command = new SqlCommand(selectSql, connection))
                {
                    using (IDataReader dataReader = command.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
                    {
                        schemaTable = dataReader.GetSchemaTable();
                    }
                }
            }

            DbSchemaRow[] schemaRows = GetSortedSchemaRows(schemaTable, false);
            return schemaRows;
        }

        private static DbSchemaRow[] GetSortedSchemaRows(DataTable dataTable, bool returnProviderSpecificTypes)
        {
            DataColumn column = dataTable.Columns[SchemaMappingUnsortedIndex];
            if (column == null)
            {
                column = new DataColumn(SchemaMappingUnsortedIndex, typeof (int));
                dataTable.Columns.Add(column);
            }
            int count = dataTable.Rows.Count;
            for (int index = 0; index < count; ++index)
            {
                dataTable.Rows[index][column] = index;
            }
            var schemaTable = new DbSchemaTable(dataTable, returnProviderSpecificTypes);
            DataRow[] dataRowArray = dataTable.Select(null, "ColumnOrdinal ASC", DataViewRowState.CurrentRows);
            var dbSchemaRowArray = new DbSchemaRow[dataRowArray.Length];
            for (int index = 0; index < dataRowArray.Length; ++index)
            {
                dbSchemaRowArray[index] = new DbSchemaRow(schemaTable, dataRowArray[index]);
            }
            return dbSchemaRowArray;
        }
    }
}