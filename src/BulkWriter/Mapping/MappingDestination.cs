namespace BulkWriter.Mapping
{
    /// <summary>
    /// Defines a destination property for mapping via an <see cref="EnumerableDataReader{TResult}"/>
    /// </summary>
    public class MappingDestination
    {
        /// <summary>
        /// Name of the column that will be presented within the <see cref="EnumerableDataReader{TResult}"/>
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 0-based index of the column that will be presented within the <see cref="EnumerableDataReader{TResult}"/>
        /// </summary>
        public int ColumnOrdinal { get; set; }

        /// <summary>
        /// Size of the column in bytes (for variable length string columns)
        /// </summary>
        public int ColumnSize { get; set; }

        /// <summary>
        /// The database provider-specific data type of the column the property is mapped to.
        /// </summary>
        public string DataTypeName { get; set; }

        /// <summary>
        /// True if the column is part of the primary key, false otherwise
        /// </summary>
        public bool IsKey { get; set; }
    }
}