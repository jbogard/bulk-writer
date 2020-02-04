using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkWriter.Pipeline;
using BulkWriter.Pipeline.Transforms;
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

        public class PipelineTestsOtherTestClass
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
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

        [Fact]
        public async Task RunsAggregatorStep()
        {
            using (var writer = new TestBulkWriter<int>())
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline.StartWith(items)
                    .Aggregate(f => f.Sum(c => c.Id))
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                Assert.Single(writer.ItemsWritten);
                Assert.Equal(Enumerable.Range(1,1000).Sum(), writer.ItemsWritten[0]);
            }
        }

        [Fact]
        public async Task RunsPivotStep()
        {
            using (var writer = new TestBulkWriter<PipelineTestsMyTestClass>())
            {
                var idCounter = 0;
                var items = Enumerable.Range(1, 10).ToList();
                var pipeline = EtlPipeline.StartWith(items)
                    .Pivot(i =>
                    {
                        var result = new List<PipelineTestsMyTestClass>();
                        for (var j = 0; j < i; j++)
                        {
                            ++idCounter;
                            result.Add(new PipelineTestsMyTestClass { Id = idCounter, Name = $"Bob {idCounter}"});
                        }
                        return result;
                    })
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                var expectedSum = items.Sum();
                Assert.Equal(expectedSum, writer.ItemsWritten.Count);
                Assert.Equal(1, writer.ItemsWritten[0].Id);
                Assert.Equal("Bob 1", writer.ItemsWritten[0].Name);
                Assert.Equal(expectedSum, writer.ItemsWritten[expectedSum - 1].Id);
                Assert.Equal($"Bob {expectedSum}", writer.ItemsWritten[expectedSum - 1].Name);
            }
        }

        [Fact]
        public async Task RunsProjectorStep()
        {
            using (var writer = new TestBulkWriter<PipelineTestsMyTestClass>())
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsOtherTestClass { Id = i, FirstName = "Bob", LastName = $"{i}"});
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .Project(i => new PipelineTestsMyTestClass { Id = i.Id, Name = $"{i.FirstName} {i.LastName}"})
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                Assert.Equal(1000, writer.ItemsWritten.Count);
                Assert.Equal(1, writer.ItemsWritten[0].Id);
                Assert.Equal("Bob 1", writer.ItemsWritten[0].Name);
                Assert.Equal(1000, writer.ItemsWritten[999].Id);
                Assert.Equal("Bob 1000", writer.ItemsWritten[999].Name);
            }
        }

        [Fact]
        public async Task RunsTransformerStep()
        {
            using (var writer = new TestBulkWriter<PipelineTestsMyTestClass>())
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .TransformInPlace(i => 
                    { 
                        i.Id -= 1;
                        i.Name = $"Alice {i.Id}";
                    })
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                Assert.Equal(1000, writer.ItemsWritten.Count);
                Assert.Equal(0, writer.ItemsWritten[0].Id);
                Assert.Equal("Alice 0", writer.ItemsWritten[0].Name);
                Assert.Equal(999, writer.ItemsWritten[999].Id);
                Assert.Equal("Alice 999", writer.ItemsWritten[999].Name);
            }
        }

        [Fact]
        public async Task RunsAllStepsInMultiStagePipeline()
        {
            using (var writer = new TestBulkWriter<PipelineTestsOtherTestClass>())
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .Aggregate(f => f.Max(c => c.Id))
                    .Pivot(i =>
                    {
                        var result = new List<PipelineTestsMyTestClass>();
                        for (var j = 1; j <= i; j++)
                        {
                            result.Add(new PipelineTestsMyTestClass { Id = j, Name = $"Bob {j}" });
                        }
                        return result;
                    })
                    .Project(i =>
                    {
                        var nameParts = i.Name.Split(' ');
                        return new PipelineTestsOtherTestClass {Id = i.Id, FirstName = nameParts[0], LastName = nameParts[1] };
                    })
                    .TransformInPlace(i =>
                    {
                        i.Id -= 1;
                        i.FirstName = "Alice";
                        i.LastName = $"{i.Id}";
                    })
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                Assert.Equal(1000, writer.ItemsWritten.Count);
                Assert.Equal(0, writer.ItemsWritten[0].Id);
                Assert.Equal("Alice", writer.ItemsWritten[0].FirstName);
                Assert.Equal("0", writer.ItemsWritten[0].LastName);
                Assert.Equal(999, writer.ItemsWritten[999].Id);
                Assert.Equal("Alice", writer.ItemsWritten[999].FirstName);
                Assert.Equal("999", writer.ItemsWritten[999].LastName);
            }
        }

        private class TestBulkWriter<T> : IBulkWriter<T>
        {
            public List<T> ItemsWritten { get; } = new List<T>();

            public void Dispose()
            {
            }

            public void WriteToDatabase(IEnumerable<T> items)
            {
                ItemsWritten.AddRange(items);
            }

            public Task WriteToDatabaseAsync(IEnumerable<T> items)
            {
                ItemsWritten.AddRange(items);
                return Task.CompletedTask;
            }
        }
    }
}
