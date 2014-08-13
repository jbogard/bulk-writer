namespace Headspring.BulkWriter
{
    using System;
    using System.Data;

    public class BulkWriterOptions
    {
        static BulkWriterOptions()
        {
            Default = new Lazy<BulkWriterOptions>(CreateDefaultOptions).Value;
        }

        private static BulkWriterOptions CreateDefaultOptions()
        {
            return new BulkWriterOptions
            {
                IsolationLevel = IsolationLevel.Snapshot,
            };
        }

        public static BulkWriterOptions Default { get; private set; }

        public IsolationLevel IsolationLevel { get; set; }
    }
}