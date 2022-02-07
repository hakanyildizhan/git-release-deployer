namespace ReleaseDeployerService.Core
{
    public struct DeployArgs
    {
        public string DeployablesPath { get; set; }
        public DateTime DeployablesCreatedAt { get; set; }
    }
}
