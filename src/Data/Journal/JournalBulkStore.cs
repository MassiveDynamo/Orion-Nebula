using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Models;
using EFCore.BulkExtensions;

namespace Data.Journal
{
    // Bulk store that uses EFCore.BulkExtensions for high-performance inserts and falls back to AddRange/SaveChanges when necessary.
    public class JournalBulkStore
    {
        private readonly Func<OrionDbContext> _contextFactory;
        private readonly Serilog.ILogger _logger;

        public BulkConfig BulkConfig { get; set; } = new BulkConfig { PreserveInsertOrder = false };

        public JournalBulkStore(Func<OrionDbContext> contextFactory, Serilog.ILogger logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StoreBatchAsync(List<object> batch, CancellationToken cancellation = default)
        {
            if (batch == null || batch.Count == 0) return;

            // Group by runtime type so we can bulk-insert per entity type
            var groups = batch.GroupBy(x => x.GetType());

            foreach (var g in groups)
            {
                cancellation.ThrowIfCancellationRequested();
                var type = g.Key;

                // Create a List<T> of the concrete type and populate it
                var listType = typeof(List<>).MakeGenericType(type);
                var list = (IList)Activator.CreateInstance(listType)!;
                foreach (var item in g) list.Add(item);

                using var context = _contextFactory();

                // Perform the bulk insert with retry/backoff, prefer BulkExtensions BulkInsertAsync
                var attempts = 0;
                var maxAttempts = 3;
                var delay = TimeSpan.FromSeconds(1);
                while (true)
                {
                    attempts++;
                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();

                        // Use EFCore.BulkExtensions generic API to invoke BulkInsertAsync<T>
                        var method = typeof(DbContextBulkExtensions).GetMethods()
                            .FirstOrDefault(m => m.Name == "BulkInsertAsync" && m.IsGenericMethodDefinition && m.GetParameters().Length >= 2);

                        if (method != null)
                        {
                            var generic = method.MakeGenericMethod(type);
                            var task = (Task)generic.Invoke(null, new object[] { context, list, BulkConfig, cancellation })!;
                            await task.ConfigureAwait(false);
                        }
                        else
                        {
                            // Fallback: AddRange + SaveChanges
                            context.AddRange(list.Cast<object>());
                            await context.SaveChangesAsync(cancellation).ConfigureAwait(false);
                        }

                        sw.Stop();
                        _logger.Information("Bulk inserted {Count} items of {Type} in {Elapsed}ms (attempt {Attempt})", list.Count, type.Name, sw.ElapsedMilliseconds, attempts);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Bulk insert attempt {Attempt} for {Type} failed", attempts, type.Name);
                        if (attempts >= maxAttempts)
                        {
                            _logger.Error(ex, "Bulk insert failed after {Attempts} attempts for {Type}. Falling back to single inserts.", attempts, type.Name);
                            // Try single inserts as final fallback
                            try
                            {
                                context.AddRange(list.Cast<object>());
                                await context.SaveChangesAsync(cancellation).ConfigureAwait(false);
                            }
                            catch (Exception inner)
                            {
                                _logger.Error(inner, "Final fallback AddRange failed for {Type}", type.Name);
                            }
                            break;
                        }

                        await Task.Delay(delay, cancellation).ConfigureAwait(false);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                    }
                }
            }
        }
    }
}
