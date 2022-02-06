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
                    config.AddLog4Net("log4net.config", true);
                    config.SetMinimumLevel(LogLevel.Debug);
                }).
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                });
        }
    }
}


