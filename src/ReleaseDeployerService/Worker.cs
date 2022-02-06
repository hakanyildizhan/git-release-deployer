using ReleaseDeployerService.Core;

namespace ReleaseDeployerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Timer _timer;
        private IAssetDownloader _assetDownloader;
        private IDeployer _deployer;
        private IConfigReader _configReader;
        public Worker(ILogger<Worker> logger,
            IConfigReader configReader,
            IAssetDownloader assetDownloader,
            IDeployer deployer)
        {
            _logger = logger;
            _configReader = configReader;
            _assetDownloader = assetDownloader;
            _deployer = deployer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExecuteAsync started.");
            _timer = new Timer(DoWork, stoppingToken, TimeSpan.Zero, TimeSpan.FromMinutes(_configReader.GetCheckInterval()));
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        private void DoWork(object state)
        {
            _logger.LogInformation($"Work started at {DateTime.Now}");
            CancellationToken cancellationToken = (CancellationToken)state;

            Task t = Task.Run(async () =>
            {
                try
                {
                    var downloadedAsset = await _assetDownloader.DownloadAsync();

                    if (downloadedAsset == null)
                    {
                        _logger.LogError($"Assets could not be downloaded.");
                        return;
                    }

                    var extractor = ExtractorFactory.GetExtractor(downloadedAsset);
                    string deployablesDir = extractor.ExtractToDirectory();

                    var deploySuccess = _deployer.Deploy(deployablesDir);
                    if (deploySuccess) _logger.LogInformation($"Deploy succeeded");
                    else _logger.LogError($"Deploy failed");
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"Error: {e.Message}\r\n{e.StackTrace}");
                    throw;
                }

            }, cancellationToken);

            t.Wait(cancellationToken);
        }
    }
}