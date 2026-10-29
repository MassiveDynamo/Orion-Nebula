using Data;
using Data.Journal;
using Data.Tests.Fixtures;
using Data.Tests.Helpers;
using Serilog;
using Serilog.Core;
using Xunit;

namespace Data.Tests
{
    /// <summary>
    /// Integration tests using the full JournalProcessor pipeline.
    /// 
    /// These tests verify that:
    /// - Real journal log files can be imported end-to-end
    /// - The JournalProcessor correctly deserializes events
    /// - Events are properly batched and persisted
    /// </summary>
    public class JournalProcessorIntegrationTests : IDisposable
    {
        private readonly TestDatabaseFixture _dbFixture;
        private readonly ILogger _logger;
        private readonly JournalBulkStore _bulkStore;

        public JournalProcessorIntegrationTests()
        {
            _dbFixture = new TestDatabaseFixture();

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            // Create bulk store with factory that creates fresh contexts for bulk operations
            _bulkStore = new JournalBulkStore(_dbFixture.CreateBulkStoreContextFactory(), _logger);
        }

        public void Dispose()
        {
            _dbFixture?.Dispose();
        }

        [Fact]
        public async Task JournalProcessor_ImportFSDJump_100Events_FromLogFile()
        {
            // Arrange: Generate synthetic FSDJump log file
            var jsonLines = TestDataGenerator.GenerateFSDJumpBatch(100);
            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                await JournalProcessor.ProcessFileAsync(logFilePath, storeBatchAsync);

                // Assert
                var storedCount = _dbFixture.DbContext.FSDJump.Count();
                Assert.Equal(100, storedCount);

                var allEvents = _dbFixture.DbContext.FSDJump.ToList();
                Assert.All(allEvents, e => 
                {
                    Assert.Equal("FSDJump", e.Event);
                    Assert.NotEmpty(e.StarSystem);
                });
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }

        [Fact]
        public async Task JournalProcessor_ImportFSSAllBodiesFound_100Events_FromLogFile()
        {
            // Arrange
            var jsonLines = TestDataGenerator.GenerateFSSAllBodiesFoundBatch(100);
            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                await JournalProcessor.ProcessFileAsync(logFilePath, storeBatchAsync);

                // Assert
                var storedCount = _dbFixture.DbContext.FSSAllBodiesFound.Count();
                Assert.Equal(100, storedCount);
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }

        [Fact]
        public async Task JournalProcessor_ImportSupercruiseExit_100Events_FromLogFile()
        {
            // Arrange
            var jsonLines = TestDataGenerator.GenerateSupercruiseExitBatch(100);
            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                await JournalProcessor.ProcessFileAsync(logFilePath, storeBatchAsync);

                // Assert
                var storedCount = _dbFixture.DbContext.SupercruiseExit.Count();
                Assert.Equal(100, storedCount);
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }

        [Fact]
        public async Task JournalProcessor_ImportProspectedAsteroid_100Events_FromLogFile()
        {
            // Arrange
            var jsonLines = TestDataGenerator.GenerateProspectedAsteroidBatch(100);
            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                await JournalProcessor.ProcessFileAsync(logFilePath, storeBatchAsync);

                // Assert
                var storedCount = _dbFixture.DbContext.ProspectedAsteroid.Count();
                Assert.Equal(100, storedCount);
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }

        [Fact]
        public async Task JournalProcessor_ImportMiningRefined_100Events_FromLogFile()
        {
            // Arrange
            var jsonLines = TestDataGenerator.GenerateMiningRefinedBatch(100);
            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                await JournalProcessor.ProcessFileAsync(logFilePath, storeBatchAsync);

                // Assert
                var storedCount = _dbFixture.DbContext.MiningRefined.Count();
                Assert.Equal(100, storedCount);
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }

        [Fact]
        public async Task JournalProcessor_ImportMixedEvents_500Total_FromLogFile()
        {
            // Arrange: Create a log file with 100 of each event type
            var jsonLines = new List<string>();
            jsonLines.AddRange(TestDataGenerator.GenerateFSDJumpBatch(100));
            jsonLines.AddRange(TestDataGenerator.GenerateFSSAllBodiesFoundBatch(100));
            jsonLines.AddRange(TestDataGenerator.GenerateSupercruiseExitBatch(100));
            jsonLines.AddRange(TestDataGenerator.GenerateProspectedAsteroidBatch(100));
            jsonLines.AddRange(TestDataGenerator.GenerateMiningRefinedBatch(100));

            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                await JournalProcessor.ProcessFileAsync(logFilePath, storeBatchAsync);

                // Assert: Each event type should have 100 entries
                Assert.Equal(100, _dbFixture.DbContext.FSDJump.Count());
                Assert.Equal(100, _dbFixture.DbContext.FSSAllBodiesFound.Count());
                Assert.Equal(100, _dbFixture.DbContext.SupercruiseExit.Count());
                Assert.Equal(100, _dbFixture.DbContext.ProspectedAsteroid.Count());
                Assert.Equal(100, _dbFixture.DbContext.MiningRefined.Count());

                // Total: 500 events
                var totalCount = _dbFixture.DbContext.FSDJump.Count()
                    + _dbFixture.DbContext.FSSAllBodiesFound.Count()
                    + _dbFixture.DbContext.SupercruiseExit.Count()
                    + _dbFixture.DbContext.ProspectedAsteroid.Count()
                    + _dbFixture.DbContext.MiningRefined.Count();

                Assert.Equal(500, totalCount);
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }

        [Fact]
        public async Task JournalProcessor_WithFailureHandler_Captures_FailedBatches()
        {
            // Arrange
            var jsonLines = TestDataGenerator.GenerateFSDJumpBatch(100);
            var logFilePath = TestDataGenerator.CreateTemporaryLogFile(jsonLines);
            var failedBatches = new List<(List<object> batch, Exception ex)>();

            try
            {
                // Act
                var storeBatchAsync = async (List<object> batch) =>
                {
                    await _bulkStore.StoreBatchAsync(batch);
                };

                var failureHandler = async (List<object> batch, Exception ex, string source) =>
                {
                    failedBatches.Add((batch, ex));
                    await Task.CompletedTask;
                };

                await JournalProcessor.ProcessFileAsync(
                    logFilePath, 
                    storeBatchAsync, 
                    failureHandler);

                // Assert: No failures expected for valid data
                Assert.Empty(failedBatches);
                Assert.Equal(100, _dbFixture.DbContext.FSDJump.Count());
            }
            finally
            {
                TestDataGenerator.DeleteTemporaryLogFile(logFilePath);
            }
        }
    }
}
