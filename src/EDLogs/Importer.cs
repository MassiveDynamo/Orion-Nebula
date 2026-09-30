using Data;
using Data.Journal;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;

namespace EDLogs
{
    public class Importer
    {
        private string _logPath;
        private AppSettings.Settings _appSettings;
        private ILogger _logger;
        private readonly OrionDbContext _dbContext;
        private readonly JournalBulkStore _bulkStore;
        private readonly FailedBatchStore _failedBatchStore;

        public Importer(ILogger logger, AppSettings.Settings appSettings, OrionDbContext dbContext, JournalBulkStore bulkStore, FailedBatchStore failedBatchStore)
        {
            _logPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Saved Games\Frontier Developments\Elite Dangerous";
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _bulkStore = bulkStore ?? throw new ArgumentNullException(nameof(bulkStore));
            _failedBatchStore = failedBatchStore ?? throw new ArgumentNullException(nameof(failedBatchStore));
        }

        public void ImportOrionNebulaSystems()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Get the solution root folder
                var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
                var nebulaDirectory = Path.Combine(solutionRoot, "Data", "Spansh");
                var systems = GetOrionNebulaSystems(nebulaDirectory);

                // 1) get existing names from DB (no tracking)
                var existingNames = new HashSet<string>(
                    _dbContext.EDSystemName
                        .AsNoTracking()
                        .Select(e => e.Name)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase // adjust comparer to DB collation if needed
                );

                _logger.Information("Found {ExistingCount} existing Orion Nebula systems in the database.", existingNames.Count);

                // 2) also exclude entities already tracked in this DbContext (if DbContext is long-lived)
                var trackedNames = new HashSet<string>(
                    _dbContext.ChangeTracker
                        .Entries<EDSystemName>()
                        .Select(e => e.Entity.Name),
                    StringComparer.OrdinalIgnoreCase
                );

                // 3) dedupe input and filter
                var newSystems = systems
                    .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .Where(s => !existingNames.Contains(s.Name) && !trackedNames.Contains(s.Name))
                    .ToArray();

                // 4) add only missing systems
                if (newSystems.Length > 0)
                {
                    _dbContext.EDSystemName.AddRange(newSystems);
                    _dbContext.SaveChanges();
                }

                _logger.Information("Imported {SystemCount} Orion Nebula systems.", newSystems.Length);
            }
            catch (Exception ex)
            {
                _logger.Error("Error importing Orion Nebula systems: {ErrorMessage}", ex.Message);
            }
            finally
            {
                sw.Stop();
                _logger.Information("Orion Nebula import process completed in {ElapsedTime} ms.", sw.ElapsedMilliseconds);
            }
        }

        private EDSystemName[] GetOrionNebulaSystems(string nebulaDirectory)
        {
            _logger.Information("Import Orion Nebula systems from the csv files in the Spansh directory");
            if(!Directory.Exists(nebulaDirectory))
            {
                _logger.Error("Spansh directory does not exist: {NebulaDirectory}. Please check the directory path.", nebulaDirectory);
                return Array.Empty<EDSystemName>();
            }

            var systems = new List<EDSystemName>();
            var csvFiles = Directory.GetFiles(nebulaDirectory, "*.csv", SearchOption.AllDirectories);
            foreach (var csvFile in csvFiles)
            {
                // First line of the csv file is the header, so skip it
                var lines = File.ReadAllLines(csvFile);
                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(',');
                    if (columns.Length >= 2)
                    {
                        var systemName = columns[0].Trim();
                        systems.Add(new EDSystemName
                        {
                            Name = systemName
                        });
                    }
                }
            }

            return systems.ToArray();
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

            // Get the latest FSDJump event timestamp from the database to avoid re-importing old logs
            var latestFSDJumpTimestamp = await _dbContext.FSDJump
                .AsNoTracking()
                .OrderByDescending(f => f.Timestamp)
                .Select(f => (DateTime?)f.Timestamp)
                .FirstOrDefaultAsync() ?? DateTime.MinValue;

            // Filter log files to only include those modified after the latest FSDJump event timestamp
            // TODO
            logFiles = logFiles.Where(logFile => File.GetLastWriteTimeUtc(logFile) > latestFSDJumpTimestamp).ToArray();
            _logger.Information("Filtered to {LogCount} log files after {LatestFSDJumpTimestamp}.", logFiles.Length, latestFSDJumpTimestamp);

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
