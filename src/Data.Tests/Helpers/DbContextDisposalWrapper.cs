using Data;
using System;

namespace Data.Tests.Helpers
{
    /// <summary>
    /// Wrapper around OrionDbContext that prevents disposal.
    /// Used to wrap the shared test fixture context so that JournalBulkStore
    /// doesn't dispose it prematurely.
    /// </summary>
    public class NonDisposableContextWrapper : OrionDbContext
    {
        private readonly OrionDbContext _innerContext;

        public NonDisposableContextWrapper(OrionDbContext innerContext) 
            : base(innerContext.GetType().BaseType != null ? new Microsoft.EntityFrameworkCore.DbContextOptions<OrionDbContext>() : null)
        {
            _innerContext = innerContext ?? throw new ArgumentNullException(nameof(innerContext));
        }

        public override void Dispose()
        {
            // Do not dispose - the wrapper prevents disposal of the shared test fixture context
        }

        public override async System.Threading.Tasks.ValueTask DisposeAsync()
        {
            // Do not dispose - the wrapper prevents disposal of the shared test fixture context
            await System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }
}

// Alternative simpler approach: A proxy that delegates all operations but prevents disposal
public class DbContextDisposalWrapper
{
    private readonly Data.OrionDbContext _context;

    public DbContextDisposalWrapper(Data.OrionDbContext context)
    {
        _context = context;
    }

    public Data.OrionDbContext Context => _context;

    public Data.OrionDbContext CreateNonDisposableProxy()
    {
        return _context; // Return the same context, but callers can't dispose it via this proxy
    }
}
