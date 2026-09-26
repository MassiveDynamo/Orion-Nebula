using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.Journal
{
    public class FailedBatchStore
    {
        private readonly string _folder;
        private readonly Func<OrionDbContext> _contextFactory;
        private readonly Serilog.ILogger _logger;

        public FailedBatchStore(Func<OrionDbContext> contextFactory, Serilog.ILogger logger, string? folder = null)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _folder = folder ?? Path.Combine(Directory.GetCurrentDirectory(), "FailedBatches");
            Directory.CreateDirectory(_folder);
        }

        public async Task SaveFailedBatchAsync(List<object> batch, Exception ex, string sourceFile)
        {
            try
            {
                // Save to DB
                try
                {
                    using var context = _contextFactory();
                    var fb = new Data.Models.FailedBatch
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceFile = sourceFile ?? string.Empty,
                        ErrorType = ex.GetType().FullName ?? string.Empty,
                        ErrorMessage = ex.Message,
                        ItemCount = batch.Count,
                        RawPayload = string.Join("\n", batch.Select(item => GetRawJsonOrSerialize(item))),
                        Status = "Pending",
                        ReplayAttempts = 0,
                        ReplayedAt = null
                    };
                    context.FailedBatch.Add(fb);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception dbEx)
                {
                    _logger.Warning(dbEx, "Saving failed batch to DB failed, will fallback to disk.");
                }

                // Also save to disk for manual replay
                var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                var outPath = Path.Combine(_folder, $"{fileName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.jsonl");
                await using var fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await using var sw = new StreamWriter(fs);

                // Write header
                await sw.WriteLineAsync($"# FailedBatch saved: {DateTime.UtcNow:o}");
                await sw.WriteLineAsync($"# SourceFile: {sourceFile}");
                await sw.WriteLineAsync($"# Exception: {ex.GetType().FullName}: {ex.Message}");

                foreach (var item in batch)
                {
                    var line = GetRawJsonOrSerialize(item);
                    await sw.WriteLineAsync(line);
                }

                await sw.FlushAsync();
            }
            catch (Exception saveEx)
            {
                // Swallow exceptions but log them
                _logger.Error(saveEx, "Failed to persist failed batch to disk or DB");
            }
        }

        private static string GetRawJsonOrSerialize(object item)
        {
            var t = item.GetType();
            var prop = t.GetProperty("RawJson", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(string))
            {
                var raw = prop.GetValue(item) as string;
                if (!string.IsNullOrEmpty(raw)) return raw;
            }
            return JsonSerializer.Serialize(item);
        }
    }
}
