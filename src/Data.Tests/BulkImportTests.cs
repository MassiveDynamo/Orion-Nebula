using Data.Journal;
using Data.Models;
using Data.Tests.Fixtures;
using Serilog;
using Xunit;

namespace Data.Tests
{
    /// <summary>
    /// Integration tests for bulk import of ED events.
    /// 
    /// These tests verify that:
    /// - 100 events of each type can be imported successfully
    /// - Bulk insert operations work correctly
    /// - No data is corrupted during import
    /// - Events are properly deserialized and stored
    /// - No database pollution occurs (tests are isolated)
    /// </summary>
    public class BulkImportTests : IDisposable
    {
        private readonly TestDatabaseFixture _dbFixture;
        private readonly ILogger _logger;
        private readonly JournalBulkStore _bulkStore;

        public BulkImportTests()
        {
            _dbFixture = new TestDatabaseFixture();

            // Create a Serilog logger for bulk store
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            // Create bulk store with factory that creates fresh contexts for bulk operations
            // This allows JournalBulkStore to dispose contexts safely
            _bulkStore = new JournalBulkStore(_dbFixture.CreateBulkStoreContextFactory(), _logger);
        }

        public void Dispose()
        {
            _dbFixture?.Dispose();
        }

        [Fact]
        public async Task BulkImport_FSDJump_100Events_Success()
        {
            // Arrange
            const int eventCount = 100;
            var batch = new List<object>();

            for (int i = 0; i < eventCount; i++)
            {
                var fsdJump = new FSDJump
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(-1000 + i),
                    Event = "FSDJump",
                    StarSystem = $"Test System {i}",
                    SystemAddress = 1000000000L + i,
                    StarPosX = 10.5 + i * 0.1,
                    StarPosY = 20.3 - i * 0.05,
                    StarPosZ = 30.1 + i * 0.02,
                    JumpDist = 5.5 + (i % 10) * 0.5,
                    FuelUsed = 2.5,
                    FuelLevel = 25.0,
                    StarClass = "G"
                };
                batch.Add(fsdJump);
            }

            // Act
            await _bulkStore.StoreBatchAsync(batch);

            // Assert
            var storedCount = _dbFixture.DbContext.FSDJump.Count();
            Assert.Equal(eventCount, storedCount);

            // Verify data integrity
            var firstEvent = _dbFixture.DbContext.FSDJump.OrderBy(e => e.Timestamp).First();
            Assert.Equal("Test System 0", firstEvent.StarSystem);
            Assert.Equal(1000000000L, firstEvent.SystemAddress);

            var lastEvent = _dbFixture.DbContext.FSDJump.OrderBy(e => e.Timestamp).Last();
            Assert.Equal($"Test System {eventCount - 1}", lastEvent.StarSystem);
            Assert.Equal(1000000000L + eventCount - 1, lastEvent.SystemAddress);
        }

        [Fact]
        public async Task BulkImport_FSSAllBodiesFound_100Events_Success()
        {
            // Arrange
            const int eventCount = 100;
            var batch = new List<object>();

            for (int i = 0; i < eventCount; i++)
            {
                var fssEvent = new FSSAllBodiesFound
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(-500 + i),
                    Event = "FSSAllBodiesFound",
                    SystemName = $"Test System {i}",
                    SystemAddress = 2000000000L + i,
                    BodyCount = 15 + (i % 5)
                };
                batch.Add(fssEvent);
            }

            // Act
            await _bulkStore.StoreBatchAsync(batch);

            // Assert
            var storedCount = _dbFixture.DbContext.FSSAllBodiesFound.Count();
            Assert.Equal(eventCount, storedCount);

            // Verify data integrity
            var firstEvent = _dbFixture.DbContext.FSSAllBodiesFound.OrderBy(e => e.Timestamp).First();
            Assert.Equal(2000000000L, firstEvent.SystemAddress);

            var lastEvent = _dbFixture.DbContext.FSSAllBodiesFound.OrderBy(e => e.Timestamp).Last();
            Assert.Equal(2000000000L + eventCount - 1, lastEvent.SystemAddress);
        }

        [Fact]
        public async Task BulkImport_SupercruiseExit_100Events_Success()
        {
            // Arrange
            const int eventCount = 100;
            var batch = new List<object>();

            for (int i = 0; i < eventCount; i++)
            {
                var scEvent = new SupercruiseExit
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(-300 + i),
                    SystemAddress = 2500000000L + i,
                    StarSystem = $"System {i}",
                    Body = $"Body {i}",
                    BodyID = 1 + (i % 20),
                    BodyType = i % 2 == 0 ? "Planet" : "Star"
                };
                batch.Add(scEvent);
            }

            // Act
            await _bulkStore.StoreBatchAsync(batch);

            // Assert
            var storedCount = _dbFixture.DbContext.SupercruiseExit.Count();
            Assert.Equal(eventCount, storedCount);

            // Verify data integrity
            var events = _dbFixture.DbContext.SupercruiseExit.OrderBy(e => e.Timestamp).ToList();
            Assert.Equal("System 0", events.First().StarSystem);
            Assert.Equal($"System {eventCount - 1}", events.Last().StarSystem);
        }

        [Fact]
        public async Task BulkImport_ProspectedAsteroid_100Events_Success()
        {
            // Arrange
            const int eventCount = 100;
            var batch = new List<object>();

            for (int i = 0; i < eventCount; i++)
            {
                var prospectEvent = new ProspectedAsteroid
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(-200 + i),
                    Event = "ProspectedAsteroid",
                    Content = "Common",
                    Materials = "[{\"Name\":\"Iron\",\"Percent\":20.5}]"
                };
                batch.Add(prospectEvent);
            }

            // Act
            await _bulkStore.StoreBatchAsync(batch);

            // Assert
            var storedCount = _dbFixture.DbContext.ProspectedAsteroid.Count();
            Assert.Equal(eventCount, storedCount);

            // Verify data integrity
            var firstEvent = _dbFixture.DbContext.ProspectedAsteroid.OrderBy(e => e.Timestamp).First();
            Assert.True(firstEvent.Timestamp > DateTime.MinValue);
        }

        [Fact]
        public async Task BulkImport_MiningRefined_100Events_Success()
        {
            // Arrange
            const int eventCount = 100;
            var batch = new List<object>();
            var materials = new[] { "Painite", "Platinum", "Palladium", "Gold" };

            for (int i = 0; i < eventCount; i++)
            {
                var miningEvent = new MiningRefined
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(-100 + i),
                    Event = "MiningRefined",
                    Type = materials[i % materials.Length],
                    Type_Localised = materials[i % materials.Length]
                };
                batch.Add(miningEvent);
            }

            // Act
            await _bulkStore.StoreBatchAsync(batch);

            // Assert
            var storedCount = _dbFixture.DbContext.MiningRefined.Count();
            Assert.Equal(eventCount, storedCount);

            // Verify data integrity
            var events = _dbFixture.DbContext.MiningRefined.ToList();
            var painiteCount = events.Count(e => e.Type == "Painite");
            var platinumCount = events.Count(e => e.Type == "Platinum");
            Assert.True(painiteCount > 0);
            Assert.True(platinumCount > 0);
        }

        [Fact]
        public async Task BulkImport_MixedEventTypes_Success()
        {
            // Arrange: Create 20 events of each type for 100 total mixed events
            const int eventsPerType = 20;
            var batch = new List<object>();

            // Add FSDJump events
            for (int i = 0; i < eventsPerType; i++)
            {
                batch.Add(new FSDJump
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(i),
                    Event = "FSDJump",
                    StarSystem = $"FSD System {i}",
                    SystemAddress = 1000000000L + i,
                    StarClass = "G"
                });
            }

            // Add FSSAllBodiesFound events
            for (int i = 0; i < eventsPerType; i++)
            {
                batch.Add(new FSSAllBodiesFound
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(eventsPerType + i),
                    Event = "FSSAllBodiesFound",
                    SystemName = $"FSS System {i}",
                    SystemAddress = 2000000000L + i,
                    BodyCount = 15
                });
            }

            // Add SupercruiseExit events
            for (int i = 0; i < eventsPerType; i++)
            {
                batch.Add(new SupercruiseExit
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(eventsPerType * 2 + i),
                    SystemAddress = 2500000000L + i,
                    StarSystem = $"SC System {i}",
                    Body = $"Body {i}"
                });
            }

            // Add ProspectedAsteroid events
            for (int i = 0; i < eventsPerType; i++)
            {
                batch.Add(new ProspectedAsteroid
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(eventsPerType * 3 + i),
                    Event = "ProspectedAsteroid",
                    Content = "Common",
                    Materials = "[{\"Name\":\"Iron\",\"Percent\":20.5}]"
                });
            }

            // Add MiningRefined events
            for (int i = 0; i < eventsPerType; i++)
            {
                batch.Add(new MiningRefined
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(eventsPerType * 4 + i),
                    Event = "MiningRefined",
                    Type = "Painite",
                    Type_Localised = "Painite"
                });
            }

            // Act
            await _bulkStore.StoreBatchAsync(batch);

            // Assert
            Assert.Equal(eventsPerType, _dbFixture.DbContext.FSDJump.Count());
            Assert.Equal(eventsPerType, _dbFixture.DbContext.FSSAllBodiesFound.Count());
            Assert.Equal(eventsPerType, _dbFixture.DbContext.SupercruiseExit.Count());
            Assert.Equal(eventsPerType, _dbFixture.DbContext.ProspectedAsteroid.Count());
            Assert.Equal(eventsPerType, _dbFixture.DbContext.MiningRefined.Count());
        }

        [Fact]
        public async Task BulkImport_EmptyBatch_NoErrors()
        {
            // Arrange
            var batch = new List<object>();

            // Act & Assert - should not throw
            await _bulkStore.StoreBatchAsync(batch);
            Assert.Empty(_dbFixture.DbContext.FSDJump);
        }

        [Fact]
        public async Task BulkImport_DatabaseIsolation_TestsDoNotInterfere()
        {
            // Arrange: First import
            var batch1 = new List<object>
            {
                new FSDJump { Timestamp = DateTime.UtcNow, Event = "FSDJump", StarSystem = "Test1", StarClass = "G" }
            };

            // Act: First import
            await _bulkStore.StoreBatchAsync(batch1);
            var countAfterFirstImport = _dbFixture.DbContext.FSDJump.Count();

            // Clear for second test
            _dbFixture.ClearAllEvents();
            var countAfterClear = _dbFixture.DbContext.FSDJump.Count();

            // Arrange: Second import
            var batch2 = new List<object>
            {
                new FSDJump { Timestamp = DateTime.UtcNow, Event = "FSDJump", StarSystem = "Test2", StarClass = "K" },
                new FSDJump { Timestamp = DateTime.UtcNow, Event = "FSDJump", StarSystem = "Test3", StarClass = "M" }
            };

            // Act: Second import
            await _bulkStore.StoreBatchAsync(batch2);
            var countAfterSecondImport = _dbFixture.DbContext.FSDJump.Count();

            // Assert
            Assert.Equal(1, countAfterFirstImport);
            Assert.Equal(0, countAfterClear);
            Assert.Equal(2, countAfterSecondImport);
        }
    }
}
