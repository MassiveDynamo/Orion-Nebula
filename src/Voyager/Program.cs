using Data;
using Data.Journal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Voyager
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddCommandLine(args)
                    .Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            // Bind options using a lambda so the Configure overload that accepts Action<T> is used.
            services.Configure<AppSettings.Settings>(opts => configuration.Bind(opts));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings.Settings>>().Value);

            // Build provider after all registrations
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger>();
            var appSettings = serviceProvider.GetRequiredService<AppSettings.Settings>();
            var dbContext = serviceProvider.GetRequiredService<OrionDbContext>();
            if (dbContext == null)
            {
                logger.Error("DbContext is null");
                return 1;
            }

            try
            {
                if (dbContext.Database.CanConnect())
                {
                    logger.Information("DbContext is connected");
                }
                else
                {
                    logger.Error("DbContext is not connected");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                logger.Error("DbContext is not connected: {ex}", ex);
                return 1;
            }

            logger.Information("DataFolder:{DataFolder}", appSettings.DataFolder);
            logger.Information("DbConnection:{dbConnection}", appSettings.DbConnection);
            logger.Information("ShowProgress:{ShowProgress}", appSettings.ShowProgress);

            try
            {
                // If invoked with 'replay' command, run replay CLI and exit
                if (args.Length > 0 && args[0].Equals("replay", StringComparison.OrdinalIgnoreCase))
                {
                    var exitCode = Tools.ReplayCli.RunAsync(args.Length > 1 ? args[1..] : Array.Empty<string>(), serviceProvider).GetAwaiter().GetResult();
                    return exitCode;
                }

                var bulkStore = serviceProvider.GetRequiredService<Data.Journal.JournalBulkStore>();
                var failedStore = serviceProvider.GetRequiredService<Data.Journal.FailedBatchStore>();
                var importer = new EDLogs.Importer(logger, appSettings, dbContext, bulkStore, failedStore);
                importer.ImportLogsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                return 1;
            }

            return 0;
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            bool.TryParse(configuration["ShowProgress"], out bool showProgress);
            services.AddSingleton<ILogger>(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger());

            // Register the DbContext with the connection string from appsettings.json
            services.AddDbContext<OrionDbContext>(options =>
            {
                options.UseSqlServer(configuration["DbConnection"]);
            });

            // Also register a DbContextFactory for creating contexts on worker threads
            services.AddDbContextFactory<OrionDbContext>(options =>
            {
                options.UseSqlServer(configuration["DbConnection"]);
            });

            // Bind BulkConfig from configuration and register JournalBulkStore wired to a context factory
            var bulkConfig = new EFCore.BulkExtensions.BulkConfig();
            configuration.GetSection("BulkConfig").Bind(bulkConfig);

            services.AddSingleton(bulkConfig);

            services.AddSingleton<JournalBulkStore>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var store = new JournalBulkStore(() => sp.GetRequiredService<IDbContextFactory<OrionDbContext>>().CreateDbContext(), logger)
                {
                    BulkConfig = sp.GetRequiredService<EFCore.BulkExtensions.BulkConfig>()
                };
                return store;
            });

            // Register FailedBatchStore with DI so the importer can persist failed batches to DB and disk
            services.AddSingleton<FailedBatchStore>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                return new FailedBatchStore(() => sp.GetRequiredService<IDbContextFactory<OrionDbContext>>().CreateDbContext(), logger);
            });
        }
    }
}