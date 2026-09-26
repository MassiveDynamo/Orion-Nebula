using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Data;
using Data.Journal;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Tools
{
    // Simple replay utility: reads FailedBatch rows from DB (or files) and re-injects payloads
    // using the same JournalProcessor + JournalBulkStore pipeline.
    public static class ReplayFailedBatches
    {
        public static async Task ReplayFromDatabaseAsync(Func<OrionDbContext> contextFactory, Func<System.Collections.Generic.List<object>, Task> handleBatchAsync, int batchSize = 500, bool markReplayed = true)
        {
            using var ctx = contextFactory();
            var batches = await ctx.FailedBatch.Where(f => f.Status != "Replayed").OrderBy(f => f.Timestamp).ToListAsync().ConfigureAwait(false);
            foreach (var b in batches)
            {
                var lines = b.RawPayload.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var models = new System.Collections.Generic.List<object>(lines.Length);
                foreach (var line in lines)
                {
                    var model = Data.Journal.JournalDeserializer.DeserializeEvent(line);
                    if (model != null)
                    {
                        models.Add(model);
                    }
                }

                try
                {
                    // Dispatch in chunks
                    for (int i = 0; i < models.Count; i += batchSize)
                    {
                        var chunk = models.GetRange(i, Math.Min(batchSize, models.Count - i));
                        await handleBatchAsync(chunk).ConfigureAwait(false);
                    }

                    if (markReplayed)
                    {
                        using var updateCtx = contextFactory();
                        var fb = await updateCtx.FailedBatch.FindAsync(b.Id).ConfigureAwait(false);
                        if (fb != null)
                        {
                            fb.ReplayAttempts += 1;
                            fb.ReplayedAt = DateTime.UtcNow;
                            fb.Status = "Replayed";
                            await updateCtx.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception using Serilog
                    Log.Error(ex, "Error occurred while replaying failed batch {FailedBatchId}", b.Id);
                    using var updateCtx = contextFactory();
                    var fb = await updateCtx.FailedBatch.FindAsync(b.Id).ConfigureAwait(false);
                    if (fb != null)
                    {
                        fb.ReplayAttempts += 1;
                        fb.Status = "Error";
                        await updateCtx.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        public static async Task ReplayFromFilesAsync(string directory, Func<System.Collections.Generic.List<object>, Task> handleBatchAsync, int batchSize = 500)
        {
            if (!Directory.Exists(directory)) return;
            var files = Directory.GetFiles(directory, "*.jsonl").OrderBy(f => f).ToList();
            foreach (var file in files)
            {
                using var sr = new StreamReader(file);
                string? line;
                var models = new System.Collections.Generic.List<object>();
                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    var model = Data.Journal.JournalDeserializer.DeserializeEvent(line);
                    if (model != null)
                    {
                        models.Add(model);
                        if (models.Count >= batchSize)
                        {
                            await handleBatchAsync(models).ConfigureAwait(false);
                            models.Clear();
                        }
                    }
                }

                if (models.Count > 0)
                {
                    await handleBatchAsync(models).ConfigureAwait(false);
                }
            }
        }
    }
}
