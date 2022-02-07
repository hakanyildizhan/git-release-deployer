using System.Reflection;

namespace ReleaseDeployerService.Core
{
    public static class ServiceConfiguration
    {
        public static string APP_DIR;
        public static string CONFIG_PATH;

        static ServiceConfiguration()
        {
            if (OperatingSystem.IsWindows())
            {
                APP_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReleaseDeployerService");
                CONFIG_PATH = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "config.xml");
            }

            Directory.CreateDirectory(APP_DIR);
        }
    }
}
