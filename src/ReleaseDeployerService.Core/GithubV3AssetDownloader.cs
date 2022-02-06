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

        public Deployable Download()
        {
            throw new NotImplementedException();
        }

        public async Task<Deployable> DownloadAsync()
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

            if (lastRelease.Assets != null && lastRelease.Assets.Count > 0)
            {
                var asset = lastRelease.Assets.OrderByDescending(a => a.CreatedAt).First();
                downloadUri = $"https://api.github.com/repos/{_reader.GetGitUserName()}/{_reader.GetGitRepo()}/releases/assets/{asset.Id}";
                downloadTargetFileName = asset.Name;
            }
            else
            {
                downloadUri = lastRelease.ZipballUrl;
                downloadTargetFileName = lastRelease.Name;
                deployableType = DeployableType.SourceZipball;
            }

            _client.DefaultRequestHeaders.Remove("Accept");
            _client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
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

            return new Deployable() { Path = downloadTargetPath, DeployableType = deployableType };
        }
    }
}
