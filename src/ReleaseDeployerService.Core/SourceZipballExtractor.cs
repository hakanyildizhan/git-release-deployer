using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseDeployerService.Core
{
    public class SourceZipballExtractor : IExtractor
    {
        private readonly string _filePath;

        public SourceZipballExtractor(string filePath)
        {
            _filePath = filePath;
        }

        public string ExtractToDirectory()
        {
            using (var archive = ZipFile.Open(_filePath, ZipArchiveMode.Read))
            {
                string targetDir = Path.Combine(ServiceConfiguration.APP_DIR, archive.Entries[0].FullName.Split('/')[0]);
                if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
                archive.ExtractToDirectory(ServiceConfiguration.APP_DIR);
                return targetDir;
            }
        }
    }
}
