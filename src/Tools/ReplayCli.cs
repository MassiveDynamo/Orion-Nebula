using Data.Journal;
using Microsoft.Extensions.DependencyInjection;

namespace Tools
{
    // Simple CLI entry to replay failed batches from DB or disk.
    public static class ReplayCli
    {
        public static async Task<int> RunAsync(string[] args, IServiceProvider services)
        {
            var bulkStore = services.GetRequiredService<JournalBulkStore>();

            // Handler to store batches via JournalBulkStore
            Func<System.Collections.Generic.List<object>, Task> handleBatch = async models =>
            {
                await bulkStore.StoreBatchAsync(models);
            };

            return 0;
        }
    }
}
