using System;
using System.Data.SqlClient;
using Headspring.BulkWriter.Properties;

namespace Headspring.BulkWriter
{
    public class BulkWriterOptions
    {
        private readonly string connectionString;

        public SqlRowsCopiedEventHandler SqlRowsCopied;

        public BulkWriterOptions(string connectionString)
        {
            if (null == connectionString)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (0 == connectionString.Length)
            {
                throw new ArgumentException(Resources.Mapping_CreateBulkWriter_InvalidConnectionString, "connectionString");
            }

            this.connectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return this.connectionString; }
        }

        public int NotifyAfter { get; set; }

        public bool EnableStreaming { get; set; }

        public int BatchSize { get; set; }

        public int BulkCopyTimeout { get; set; }
    }
}