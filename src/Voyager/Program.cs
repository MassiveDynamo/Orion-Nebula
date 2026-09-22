using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Voyager
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var task = MainAsync(args);
            task.Wait();
            return task.Result;
        }

        public static async Task<int> MainAsync(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddCommandLine(args)
                    .Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger>();
            var appSettings = serviceProvider.GetRequiredService<AppSettings.Settings>();
            logger.Information("DataFolder:{DataFolder}", appSettings.DataFolder);
            logger.Information("DbConnection:{dbConnection}", appSettings.DbConnection);
            logger.Information("ShowProgress:{ShowProgress}", appSettings.ShowProgress);

            try
            {
                var importer = new EDLogs.Importer(logger, appSettings);
                importer.ImportLogs();
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
            services.AddSingleton(new AppSettings.Settings(
                configuration["DataFolder"],
                configuration["DbConnection"],
                showProgress
            ));

            services.AddSingleton<ILogger>(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger());
        }
    }
}