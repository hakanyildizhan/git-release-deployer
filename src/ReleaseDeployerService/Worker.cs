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
            _logger.LogInformation("Service is started.");
            _timer = new Timer(DoWork, stoppingToken, TimeSpan.Zero, TimeSpan.FromMinutes(_configReader.GetCheckInterval()));
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        private void DoWork(object state)
        {
            _logger.LogInformation($"Work started.");
            CancellationToken cancellationToken = (CancellationToken)state;

            Task t = Task.Run(async () =>
            {
                try
                {
                    var downloadedAsset = await _assetDownloader.DownloadAsync();

                    if (downloadedAsset == null)
                    {
                        _logger.LogInformation($"Assets were not downloaded.");
                        return;
                    }

                    var extractor = ExtractorFactory.GetExtractor(downloadedAsset.Value);
                    string deployablesDir = extractor.ExtractToDirectory();

                    var deploySuccess = _deployer.Deploy(new DeployArgs() 
                    { 
                        DeployablesPath = deployablesDir,
                        DeployablesCreatedAt = downloadedAsset.Value.CreatedAt
                    });
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error: {e.Message}\r\n{e.StackTrace}");
                }

            }, cancellationToken);

            t.Wait(cancellationToken);
        }
    }
}