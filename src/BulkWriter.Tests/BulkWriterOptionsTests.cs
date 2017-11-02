using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace BulkWriter.Tests
{
    public class BulkWriterOptionsTests
    {
        private readonly string _connectionString = TestHelpers.ConnectionString;

        public class BulkWriterSetupTestsMyTestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("TestClass2")]
        public class BulkWriterSetupTestsMyTestClassAnnotation
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Should_Setup_BulkCopy_As_Expected()
        {
            string actualTableName = string.Empty;
            int actualBatchSize = 0;
            int actualTimeOut = 0;
            bool actualEnabledStreaming = false;
            int numColumnMappings = 0;

            var options = new BulkWriterOptions
            {
                BatchSize = 20,
                BulkCopyTimeout = 55,
                BulkCopySetup = copy =>
                {
                    actualTableName = copy.DestinationTableName;
                    actualBatchSize = copy.BatchSize;
                    actualTimeOut = copy.BulkCopyTimeout;
                    actualEnabledStreaming = copy.EnableStreaming;
                    numColumnMappings = copy.ColumnMappings.Count;
                }
            };

            var ignored = new BulkWriter<BulkWriterSetupTestsMyTestClass>(_connectionString, options);

            Assert.Equal(nameof(BulkWriterSetupTestsMyTestClass), actualTableName);
            Assert.Equal(55, actualTimeOut);
            Assert.Equal(20, actualBatchSize);
            Assert.True(actualEnabledStreaming);
            Assert.Equal(2, numColumnMappings);
        }

        [Fact]
        public void Should_Setup_BulkCopy_As_Expected_Annotations()
        {
            string actualTableName = string.Empty;
            int actualBatchSize = 0;
            int actualTimeOut = 0;
            bool actualEnabledStreaming = false;
            int numColumnMappings = 0;

            var options = new BulkWriterOptions
            {
                BatchSize = 20,
                BulkCopyTimeout = 55,
                BulkCopySetup = copy =>
                {
                    actualTableName = copy.DestinationTableName;
                    actualBatchSize = copy.BatchSize;
                    actualTimeOut = copy.BulkCopyTimeout;
                    actualEnabledStreaming = copy.EnableStreaming;
                    numColumnMappings = copy.ColumnMappings.Count;
                }
            };
            var ignored = new BulkWriter<BulkWriterSetupTestsMyTestClassAnnotation>(_connectionString, options);

            Assert.Equal("TestClass2", actualTableName);
            Assert.Equal(55, actualTimeOut);
            Assert.Equal(20, actualBatchSize);
            Assert.True(actualEnabledStreaming);
            Assert.Equal(2, numColumnMappings);
        }

        [Fact]
        public void Should_Setup_BulkCopy_As_Expected_Abuse_Callback()
        {
            var options = new BulkWriterOptions
            {
                BulkCopySetup = copy =>
               {
                   copy.Close();
               }
            };
            Assert.Throws<InvalidOperationException>(() => new BulkWriter<BulkWriterSetupTestsMyTestClassAnnotation>(_connectionString, options));
        }
    }
}