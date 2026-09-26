using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Data.Journal;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics;

namespace EDLogs
{
    public class Importer
    {
        private string _logPath;
        private AppSettings.Settings _appSettings;
        private ILogger _logger;
        private readonly OrionDbContext dbContext;
        private readonly JournalBulkStore _bulkStore;
        private readonly FailedBatchStore _failedBatchStore;

        public Importer(ILogger logger, AppSettings.Settings appSettings, OrionDbContext dbContext, JournalBulkStore bulkStore, FailedBatchStore failedBatchStore)
        {
            _logPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Saved Games\Frontier Developments\Elite Dangerous";
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _bulkStore = bulkStore ?? throw new ArgumentNullException(nameof(bulkStore));
            _failedBatchStore = failedBatchStore ?? throw new ArgumentNullException(nameof(failedBatchStore));
        }

        public async Task ImportLogsAsync()
        {
            var sw = Stopwatch.StartNew();
            if (!Directory.Exists(_logPath))
            {
                _logger.Error("Log directory does not exist.");
                return;
            }

            var logFiles = Directory.GetFiles(_logPath, "*.log", SearchOption.AllDirectories);
            _logger.Information("Found {LogCount} log files.", logFiles.Length);
            var failedStore = _failedBatchStore;

            foreach (var logFile in logFiles)
            {
                try
                {
                    // Process using the high-throughput processor
                    var cts = new CancellationTokenSource();
                    Func<List<object>, Task> storeBatchAsync = batch => _bulkStore.StoreBatchAsync(batch, cts.Token);

                    // failure handler writes failed batches to disk so import can continue
                    Func<List<object>, Exception, string, Task> failureHandler = (batch, ex, source) => failedStore.SaveFailedBatchAsync(batch, ex, source);

                    // Expose worker and batch tuning via app settings if present
                    int workerCount = 0;
                    int batchSize = 0;
                    try
                    {
                        if (!string.IsNullOrEmpty(_appSettings?.BulkWorkerCount))
                            int.TryParse(_appSettings.BulkWorkerCount, out workerCount);
                        if (!string.IsNullOrEmpty(_appSettings?.BulkBatchSize))
                            int.TryParse(_appSettings.BulkBatchSize, out batchSize);
                    }
                    catch { }

                    var wc = workerCount > 0 ? workerCount : Environment.ProcessorCount;
                    var bs = batchSize > 0 ? batchSize : 1000;

                    await JournalProcessor.ProcessFileAsync(logFile, storeBatchAsync, failureHandler, wc, bs);

                    _logger.Information("Imported log file: {LogFile}", logFile);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error importing log file {LogFile}: {ErrorMessage}", logFile, ex.Message);
                }
            }

            sw.Stop();
            _logger.Information("Import process completed in {ElapsedTime} ms.", sw.ElapsedMilliseconds);   
        }
    }
}
