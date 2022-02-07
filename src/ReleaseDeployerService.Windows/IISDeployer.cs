using log4net;
using Microsoft.Web.Administration;
using ReleaseDeployerService.Core;
using System.Composition;

namespace ReleaseDeployerService.Windows
{
    [Export(typeof(IDeployer))]
    public class IISDeployer : IDeployer
    {
        private readonly ILog _logger;
        private readonly IConfigReader _reader;

        public IISDeployer(IConfigReader reader, ILogFactory logFactory)
        {
            _reader = reader;
            _logger = logFactory.Create(typeof(IISDeployer));
        }

        public bool Deploy(DeployArgs args)
        {
            if (!Directory.Exists(args.DeployablesPath) || Directory.GetFiles(args.DeployablesPath).Length == 0)
            {
                _logger.Error($"An error occurred. Directory \"{args.DeployablesPath}\" does not exist or has no files.");
                return false;
            }

            var siteName = _reader.GetDeploySite();

            // Find IIS website & physical path
            // Stop its App pool
            // copy files
            // Start its App pool

            string virtualDirName = string.Empty;

            if (siteName.Contains('/'))
            {
                virtualDirName = siteName.Split('/')[1];
                siteName = siteName.Split('/')[0];
            }

            using (ServerManager serverManager = new ServerManager())
            {
                var targetSite = serverManager.Sites.FirstOrDefault(s => s.Name.Equals(siteName));

                if (targetSite == null)
                {
                    _logger.Error($"IIS website \"{targetSite}\" could not be found.");
                    return false;
                }

                string appPoolName = string.Empty;
                string appPhysicalPath = string.Empty;

                if (!string.IsNullOrEmpty(virtualDirName))
                {
                    var virtualDir = targetSite.Applications.FirstOrDefault(a => a.Path.Equals("/" + virtualDirName));

                    if (virtualDir == null)
                    {
                        _logger.Error($"IIS virtual directory \"{virtualDirName}\" under site \"{targetSite}\" could not be found.");
                        return false;
                    }

                    appPoolName = virtualDir.ApplicationPoolName;
                    appPhysicalPath = virtualDir.VirtualDirectories[0].PhysicalPath;
                }
                else
                {
                    appPoolName = targetSite.Applications[0].ApplicationPoolName;
                    appPhysicalPath = targetSite.Applications[0].VirtualDirectories[0].PhysicalPath;
                }

                // stop app pool
                serverManager.ApplicationPools[appPoolName].Stop();
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    if (serverManager.ApplicationPools[appPoolName].State == ObjectState.Stopped)
                    {
                        break;
                    }
                }

                if (serverManager.ApplicationPools[appPoolName].State != ObjectState.Stopped)
                {
                    _logger.Fatal($"App pool {appPoolName} could not be stopped!");
                    return false;
                }

                // copy files
                var filesToCopy = Directory.GetFiles(args.DeployablesPath);

                try
                {
                    foreach (var file in filesToCopy)
                    {
                        File.Copy(file, Path.Combine(appPhysicalPath, new FileInfo(file).Name), true);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.Fatal($"Cannot copy files to target site directory due to unauthorized access: {ex.Message}");
                    return false;
                }

                // start app pool
                serverManager.ApplicationPools[appPoolName].Start();
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    if (serverManager.ApplicationPools[appPoolName].State == ObjectState.Started)
                    {
                        break;
                    }
                }

                if (serverManager.ApplicationPools[appPoolName].State != ObjectState.Started)
                {
                    _logger.Fatal($"App pool {appPoolName} could not be started!");
                    return false;
                }

                _logger.Info("Deploy succeeded.");
                _reader.SetLastDeployedReleaseDate(args.DeployablesCreatedAt);
                return true;
            }
        }
    }
}
