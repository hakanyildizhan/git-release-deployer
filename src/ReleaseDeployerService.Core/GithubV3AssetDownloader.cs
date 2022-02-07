using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ReleaseDeployerService.Core
{
    public class GithubV3AssetDownloader : IAssetDownloader
    {
        private IConfigReader _reader;
        private readonly ILogger<GithubV3AssetDownloader> _logger;
        private static HttpClient _client = new HttpClient();

        public GithubV3AssetDownloader(IConfigReader reader, ILogger<GithubV3AssetDownloader> logger)
        {
            _reader = reader;
            _logger = logger;
        }

        public Deployable? Download()
        {
            throw new NotImplementedException();
        }

        public async Task<Deployable?> DownloadAsync()
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_reader.GetGitToken()}");
            _client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _client.DefaultRequestHeaders.Add("User-Agent", _reader.GetGitRepo());
            var responseMessage = await _client.GetAsync($"https://api.github.com/repos/{_reader.GetGitUserName()}/{_reader.GetGitRepo()}/releases");

            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogError($"An error occurred while getting releases: StatusCode: {responseMessage.StatusCode}\r\nError: {responseMessage.ReasonPhrase}");
                return null;
            }

            var response = await responseMessage.Content.ReadAsStringAsync();
            var releases = JsonConvert.DeserializeObject<IList<Release>>(response);
            var lastRelease = releases.OrderByDescending(r => r.CreatedAt).First();
            string downloadTargetFileName = String.Empty;
            string downloadUri = string.Empty;
            var deployableType = DeployableType.Asset;
            var deployDate = DateTime.Now;

            if (lastRelease.Assets != null && lastRelease.Assets.Count > 0)
            {
                var asset = lastRelease.Assets.OrderByDescending(a => a.CreatedAt).First();
                downloadUri = $"https://api.github.com/repos/{_reader.GetGitUserName()}/{_reader.GetGitRepo()}/releases/assets/{asset.Id}";
                downloadTargetFileName = asset.Name;
                deployDate = asset.CreatedAt;
            }
            else
            {
                downloadUri = lastRelease.ZipballUrl;
                downloadTargetFileName = $"{_reader.GetGitRepo()}-{lastRelease.Name}.zip";
                deployableType = DeployableType.SourceZipball;
                deployDate = lastRelease.CreatedAt;
            }

            // abort if this or a later release was already deployed
            var lastDeployDate = _reader.GetLastDeployedReleaseDate();
            if (lastDeployDate != null && lastDeployDate >= deployDate)
            {
                _logger.LogInformation("No new assets since last deploy, aborting.");
                return null;
            }

            _client.DefaultRequestHeaders.Remove("Accept");
            _client.DefaultRequestHeaders.Add("Accept", deployableType == DeployableType.Asset ? "application/octet-stream" : "application/vnd.github.V3.raw");
            responseMessage = await _client.GetAsync(downloadUri);

            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogError($"An error occurred while downloading the asset: StatusCode: {responseMessage.StatusCode}\r\nError: {responseMessage.ReasonPhrase}");
                return null;
            }

            string downloadTargetPath = Path.Combine(ServiceConfiguration.APP_DIR, downloadTargetFileName);

            using (var fs = new FileStream(downloadTargetPath, FileMode.Create))
            {
                await responseMessage.Content.CopyToAsync(fs);
            }

            return new Deployable() 
            { 
                Path = downloadTargetPath, 
                DeployableType = deployableType,
                CreatedAt = deployDate
            };
        }
    }
}
