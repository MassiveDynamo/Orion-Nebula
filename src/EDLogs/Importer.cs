using Serilog;
using System.Diagnostics;

namespace EDLogs
{
    public class Importer
    {
        private string _logPath;
        private AppSettings.Settings _appSettings;
        private ILogger _logger;
        public Importer(ILogger logger, AppSettings.Settings appSettings)
        {
            _logPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Saved Games\Frontier Developments\Elite Dangerous";
            _appSettings = appSettings;
            _logger = logger;
        }

        public void ImportLogs()
        {
            var sw = Stopwatch.StartNew();
            if (!Directory.Exists(_logPath))
            {
                _logger.Error("Log directory does not exist.");
                return;
            }

            var logFiles = Directory.GetFiles(_logPath, "*.log", SearchOption.AllDirectories);
            _logger.Information("Found {LogCount} log files.", logFiles.Length);
            foreach (var logFile in logFiles)
            {
                try
                {
                    var logContent = File.ReadAllText(logFile);

                    // Process the log content as needed
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
