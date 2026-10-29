using Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Data.Tests.Fixtures
{
    /// <summary>
    /// IDisposable fixture that provides an isolated in-memory database for testing.
    /// Uses EF Core InMemory provider to avoid database provider conflicts and SQL dialect issues.
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        private readonly string _databaseName;

        public OrionDbContext DbContext { get; private set; }

        public TestDatabaseFixture()
        {
            // Create a unique in-memory database name for this test instance
            _databaseName = $"test_orion_{Guid.NewGuid()}";

            // Create DbContext with InMemory database
            DbContext = CreateDbContext();
        }

        private OrionDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OrionDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new OrionDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }

        /// <summary>
        /// Create a factory function suitable for JournalBulkStore.
        /// Each call returns a new context instance that can be safely disposed.
        /// </summary>
        public Func<OrionDbContext> CreateBulkStoreContextFactory()
        {
            return CreateDbContext;
        }

        /// <summary>
        /// Clear all event tables to start fresh for next test.
        /// </summary>
        public void ClearAllEvents()
        {
            DbContext.FSDJump.RemoveRange(DbContext.FSDJump);
            DbContext.FSSAllBodiesFound.RemoveRange(DbContext.FSSAllBodiesFound);
            DbContext.SupercruiseExit.RemoveRange(DbContext.SupercruiseExit);
            DbContext.ProspectedAsteroid.RemoveRange(DbContext.ProspectedAsteroid);
            DbContext.MiningRefined.RemoveRange(DbContext.MiningRefined);
            DbContext.SaveChanges();
        }

        /// <summary>
        /// Get count of all events across all event types.
        /// </summary>
        public int GetTotalEventCount()
        {
            var count = 0;
            count += DbContext.FSDJump.Count();
            count += DbContext.FSSAllBodiesFound.Count();
            count += DbContext.SupercruiseExit.Count();
            count += DbContext.ProspectedAsteroid.Count();
            count += DbContext.MiningRefined.Count();
            return count;
        }
    }
}

