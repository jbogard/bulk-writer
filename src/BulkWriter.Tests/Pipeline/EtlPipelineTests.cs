using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BulkWriter.Pipeline;
using Microsoft.Extensions.Logging;
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
        public async Task ThrowsWhenAStepThrows()
        {
            var tableName = TestHelpers.DropCreate(nameof(PipelineTestsMyTestClass));

            using (var writer = new BulkWriter<PipelineTestsMyTestClass>(_connectionString))
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .Project<PipelineTestsMyTestClass>(i => throw new Exception("Projection exception"))
                    .WriteTo(writer);

                var pipelineTask = pipeline.ExecuteAsync();
                var exception = await Assert.ThrowsAsync<Exception>(() => pipelineTask);
                Assert.Equal("Projection exception", exception.Message);
            }
        }

        [Fact]
        public async Task RaisesExceptionsForAllStepsThatThrow()
        {
            var tableName = TestHelpers.DropCreate(nameof(PipelineTestsMyTestClass));

            using (var writer = new BulkWriter<PipelineTestsMyTestClass>(_connectionString))
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .Project(i =>
                    {
                        //pump a few values through to ensure the next pipeline step actually
                        //gets run
                        if (i.Id >= 400)
                            throw new Exception("Projection exception 1");

                        return i;
                    })
                    .Project(i =>
                    {
                        if (i.Id >= 200)
                            throw new Exception("Projection exception 2");

                        return i;
                    })
                    .WriteTo(writer);

                var pipelineTask = pipeline.ExecuteAsync();
                await Assert.ThrowsAsync<Exception>(() => pipelineTask);

                Assert.Equal(2, pipelineTask.Exception.InnerExceptions.Count);
                Assert.Equal(1, pipelineTask.Exception.InnerExceptions.Count(e => e.Message == "Projection exception 1"));
                Assert.Equal(1, pipelineTask.Exception.InnerExceptions.Count(e => e.Message == "Projection exception 2"));
            }
        }

        [Fact]
        public async Task PartiallyCompletesWhenAStepThrows()
        {
            var tableName = TestHelpers.DropCreate(nameof(PipelineTestsMyTestClass));

            using (var writer = new BulkWriter<PipelineTestsMyTestClass>(_connectionString))
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .TransformInPlace(i =>
                    {
                        if (i.Id > 500) throw new Exception("Transform exception");
                        i.Id -= 1;
                        i.Name = $"Alice {i.Id}";
                    })
                    .WriteTo(writer);

                var pipelineTask = pipeline.ExecuteAsync();
                await Assert.ThrowsAsync<Exception>(() => pipelineTask);

                var count = (int)await TestHelpers.ExecuteScalar(_connectionString, $"SELECT COUNT(1) FROM {tableName}");
                Assert.Equal(500, count);
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

        [Fact]
        public async Task IgnoresNullLogger()
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
                    .LogWith(null)
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                Assert.Equal(1000, writer.ItemsWritten.Count);
            }
        }

        [Fact]
        public async Task LogsStartAndFinishOfPipelineSteps()
        {
            using (var writer = new TestBulkWriter<PipelineTestsOtherTestClass>())
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var loggerFactory = new FakeLoggerFactory();
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .LogWith(loggerFactory)
                    .Aggregate(f =>
                    {
                        Thread.Sleep(1);
                        return f.Max(c => c.Id);
                    })
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
                        return new PipelineTestsOtherTestClass { Id = i.Id, FirstName = nameParts[0], LastName = nameParts[1] };
                    })
                    .TransformInPlace(i =>
                    {
                        i.Id -= 1;
                        i.FirstName = "Alice";
                        i.LastName = $"{i.Id}";
                    })
                    .WriteTo(writer);

                await pipeline.ExecuteAsync();

                var totalStepsExpected = 6;
                Assert.True(2 * totalStepsExpected == loggerFactory.LoggedMessages.Count, string.Join("\r\n", loggerFactory.LoggedMessages.Select(m => m.Message))); //2 log messages for each step in the pipeline

                for (var i = 1; i <= totalStepsExpected; i++)
                {
                    var messagesForStep = loggerFactory.LoggedMessages.Where(m => m.Message.Contains($"step {i} of {totalStepsExpected}")).ToList();
                    Assert.True(messagesForStep.Count == 2, $"Found {messagesForStep.Count} messages for step {i}");

                    Assert.Equal(1, messagesForStep.Count(m => m.Message.Contains("Starting")));
                    Assert.Equal(1, messagesForStep.Count(m => m.Message.Contains("Completing")));
                }
            }
        }

        [Fact]
        public async Task LogsExceptionInPipelineStep()
        {
            using (var writer = new TestBulkWriter<PipelineTestsMyTestClass>())
            {
                var items = Enumerable.Range(1, 1000).Select(i => new PipelineTestsMyTestClass { Id = i, Name = "Bob" });
                var loggerFactory = new FakeLoggerFactory();
                var pipeline = EtlPipeline
                    .StartWith(items)
                    .Project<PipelineTestsMyTestClass>(i => throw new Exception("This is my exception"))
                    .LogWith(loggerFactory)
                    .WriteTo(writer);

                var pipelineTask = pipeline.ExecuteAsync();
                await Assert.ThrowsAsync<Exception>(() => pipelineTask);

                var errorMessages = loggerFactory.LoggedMessages.Where(m => m.LogLevel == LogLevel.Error).ToList();
                Assert.Single(errorMessages);

                Assert.Contains("Error", errorMessages[0].Message);
                Assert.Equal("This is my exception", errorMessages[0].Exception.Message);
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

        private class FakeLoggerFactory : ILoggerFactory
        {
            public List<FakeLogger> LoggerInstances { get; } = new List<FakeLogger>();
            public List<FakeLogger.LogEntry> LoggedMessages => LoggerInstances.SelectMany(i => i.LoggedMessages).ToList();
            private readonly object _lock = new object();

            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                lock (_lock)
                {
                    var logger = new FakeLogger();
                    LoggerInstances.Add(logger);

                    return logger;
                }
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }
        }

        private class FakeLogger : ILogger
        {
            public List<LogEntry> LoggedMessages { get; } = new List<LogEntry>();

            public class LogEntry
            {
                public string Message { get; set; }
                public LogLevel LogLevel { get; set; }
                public Exception Exception { get; set; }
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                LoggedMessages.Add(new LogEntry
                {
                    Exception = exception,
                    LogLevel = logLevel,
                    Message = formatter(state, exception)
                });
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new FakeLogScope();
            }

            private sealed class FakeLogScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
