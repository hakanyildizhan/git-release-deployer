using System.IO.Compression;

namespace ReleaseDeployerService.Core
{
    public class AssetExtractor : IExtractor
    {
        private readonly string _filePath;

        public AssetExtractor(string filePath)
        {
            _filePath = filePath;
        }

        public string ExtractToDirectory()
        {
            string dirName = new FileInfo(_filePath).Directory.FullName;
            string fileName = Path.GetFileNameWithoutExtension(_filePath);
            string targetDir = Path.Combine(dirName, fileName);
            if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
            ZipFile.ExtractToDirectory(_filePath, targetDir);
            return targetDir;
        }
    }
}
