using ReleaseDeployerService.Core;

namespace ReleaseDeployerService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureLogging((hostingContext, config) =>
            {
                config.AddLog4Net("log4net.config", true)
                    .SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ILogger, Log4NetLogger>()
                    .AddSingleton<IConfigReader, XmlConfigReader>()
                    .AddSingleton<IAssetDownloader, GithubV3AssetDownloader>()
                    .AddMefService<IDeployer>(ServiceLifetime.Transient)
                    .AddHostedService<Worker>();
            });
        }
    }
}


