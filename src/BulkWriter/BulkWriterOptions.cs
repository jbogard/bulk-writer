using System;
using System.Data.SqlClient;

namespace BulkWriter
{
    public class BulkWriterOptions
    {
        public int BatchSize { get; set; } = 0;
        public int BulkCopyTimeout { get; set; } = 0;

        /// <summary>
        /// Callback to hook into the <see cref="SqlBulkCopy"/> instance. Can be used to attach to <see cref="SqlBulkCopy.SqlRowsCopied"/> for instance.
        /// </summary>
        public Action<SqlBulkCopy> BulkCopySetup { get; set; } = sbc => { };
    }
}