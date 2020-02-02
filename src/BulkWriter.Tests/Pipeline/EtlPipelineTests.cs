using System;
using System.Linq;
using System.Threading.Tasks;
using BulkWriter.Pipeline;
using Xunit;

namespace BulkWriter.Tests.Pipeline
{
    public class EtlPipelineTests
    {
        private readonly string _connectionString = TestHelpers.ConnectionString;

        public class PipelineTestsMyTestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public async Task WritesToBulkWriter()
        {
            var tableName = TestHelpers.DropCreate(nameof(PipelineTestsMyTestClass));

            using (var writer = new BulkWriter<PipelineTestsMyTestClass>(_connectionString))
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                var count = (int)await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");

                Assert.Equal(1000, count);
            }
        }

        [Fact]
        public async Task RunsToCompletionWhenAStepThrows()
        {
            var tableName = TestHelpers.DropCreate(nameof(PipelineTestsMyTestClass));

            using (var writer = new BulkWriter<PipelineTestsMyTestClass>(_connectionString))
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .Project<PipelineTestsMyTestClass>(i => throw new Exception())
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                var count = (int)await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");

                Assert.Equal(0, count);
            }
        }
    }
}
