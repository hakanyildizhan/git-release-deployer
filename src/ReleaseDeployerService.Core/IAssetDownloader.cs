namespace ReleaseDeployerService.Core
{
    public interface IAssetDownloader
    {
        Deployable? Download();
        Task<Deployable?> DownloadAsync();
    }
}
