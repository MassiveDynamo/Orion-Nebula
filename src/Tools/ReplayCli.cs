using System;
using System.Threading.Tasks;
using Data;
using Data.Journal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Tools
{
    // Simple CLI entry to replay failed batches from DB or disk.
    public static class ReplayCli
    {
        public static async Task<int> RunAsync(string[] args, IServiceProvider services)
        {
            var bulkStore = services.GetRequiredService<JournalBulkStore>();
            var failedStore = services.GetRequiredService<Data.Journal.FailedBatchStore>();

            // Handler to store batches via JournalBulkStore
            Func<System.Collections.Generic.List<object>, Task> handleBatch = async models =>
            {
                await bulkStore.StoreBatchAsync(models);
            };

            var markReplayed = args.Any(a => a.Equals("--mark", StringComparison.OrdinalIgnoreCase));

            if (args.Length > 0 && args[0].Equals("files", StringComparison.OrdinalIgnoreCase))
            {
                // replay from disk
                var folder = args.Length > 1 ? args[1] : System.IO.Path.Combine(Environment.CurrentDirectory, "FailedBatches");
                await ReplayFailedBatches.ReplayFromFilesAsync(folder, handleBatch);
                return 0;
            }

            // default: replay from DB
            var ctxFactory = new Func<OrionDbContext>(() => services.GetRequiredService<IDbContextFactory<OrionDbContext>>().CreateDbContext());
            await ReplayFailedBatches.ReplayFromDatabaseAsync(ctxFactory, handleBatch, batchSize: 1000, markReplayed: markReplayed);
            return 0;
        }
    }
}
