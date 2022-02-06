namespace ReleaseDeployerService.Core
{
    public static class ExtractorFactory
    {
        public static IExtractor GetExtractor(Deployable deployable)
        {
            switch (deployable.DeployableType)
            {
                case DeployableType.SourceZipball:
                    return new SourceZipballExtractor(deployable.Path);
                case DeployableType.Asset:
                    return new AssetExtractor(deployable.Path);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
