namespace ReleaseDeployerService.Core
{
    public interface IConfigReader
    {
        bool CheckValidity();
        string GetGitUserName();
        string GetGitRepo();
        string GetGitToken();
        string GetDeployType();
        string GetDeploySite();
        uint GetCheckInterval();
        DateTime? GetLastDeployedReleaseDate();
        bool SetLastDeployedReleaseDate(string releaseDate);
    }
}
