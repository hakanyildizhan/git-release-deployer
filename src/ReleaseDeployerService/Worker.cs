using ReleaseDeployerService.Core;

namespace ReleaseDeployerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Timer _timer;
        private IAssetDownloader assetDownloader => (IAssetDownloader)UnityServiceProvider.Instance.GetService<IAssetDownloader>();
        private IDeployer _deployer => (IDeployer)UnityServiceProvider.Instance.GetService<IDeployer>();
        //private IDeployer _deployer;
        private IConfigReader configReader => (IConfigReader)UnityServiceProvider.Instance.GetService<IConfigReader>();
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            //_deployer = deployer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExecuteAsync started.");
            try
            {
                _timer = new Timer(DoWork, stoppingToken, TimeSpan.Zero, TimeSpan.FromMinutes(configReader.GetCheckInterval()));
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Error1: {e.Message}\r\n{e.StackTrace}");
                throw;
            }
            
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
                    var downloadedAsset = await assetDownloader.DownloadAsync();

                    if (downloadedAsset == null)
                    {
                        _logger.LogError($"Assets could not be downloaded.");
                        return;
                    }

                    var extractor = ExtractorFactory.GetExtractor(downloadedAsset);
                    string deployablesDir = extractor.ExtractToDirectory();

                    var deploySuccess = _deployer.Deploy(deployablesDir);
                    _logger.LogError($"Deploy {(deploySuccess ? "succeeded" : "failed")}");
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"Error2: {e.Message}\r\n{e.StackTrace}");
                    throw;
                }
                

            }, cancellationToken);

            t.Wait(cancellationToken);
        }
    }
}