using Microsoft.Data.SqlClient;

namespace BulkWriter.Internal
{
    internal static class PropertyMappingExtensions
    {
        public static SqlBulkCopyColumnMapping ToColumnMapping(this PropertyMapping self) =>
            !string.IsNullOrWhiteSpace(self.Destination.ColumnName)
                ? new SqlBulkCopyColumnMapping(self.Source.Ordinal, self.Destination.ColumnName)
                : new SqlBulkCopyColumnMapping(self.Source.Ordinal, self.Destination.ColumnOrdinal);
    }
}