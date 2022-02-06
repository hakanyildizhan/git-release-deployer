using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReleaseDeployerService.Core
{
    public class XmlConfigReader : IConfigReader
    {
        private XmlDocument _doc;
        private string _filePath;

        public XmlConfigReader(string configFilePath)
        {
            _doc = new XmlDocument();
            _doc.Load(configFilePath);
            _filePath = configFilePath;
        }

        public bool CheckValidity()
        {
            if (_doc.DocumentElement == null) return false;
            var rootNode = _doc.DocumentElement.SelectSingleNode("/serviceConfig");
            if (rootNode == null) return false;

            var githubRepoNode = rootNode.SelectSingleNode("gitRepo");
            if (githubRepoNode == null) return false;

            var repoAttrs = githubRepoNode.Attributes;
            if (repoAttrs == null || repoAttrs.Count != 2) return false;

            var userNameAttr = repoAttrs.GetNamedItem("username");
            if (userNameAttr == null) return false;
            if (string.IsNullOrEmpty(userNameAttr.Value)) return false;

            var repoAttr = repoAttrs.GetNamedItem("repo");
            if (repoAttr == null) return false;
            if (string.IsNullOrEmpty(repoAttr.Value)) return false;

            var tokenNode = githubRepoNode.SelectSingleNode("token");
            if (tokenNode == null) return false;
            if (string.IsNullOrEmpty(tokenNode.InnerText)) return false;

            var deployNode = githubRepoNode.SelectSingleNode("deploy");
            if (deployNode == null) return false;
            if (string.IsNullOrEmpty(deployNode.InnerText)) return false;

            var deployNodeAttrs = deployNode.Attributes;
            if (deployNodeAttrs == null || deployNodeAttrs.Count != 1) return false;

            var deployTypeAttr = deployNodeAttrs.GetNamedItem("type");
            if (deployTypeAttr == null) return false;
            if (string.IsNullOrEmpty(deployTypeAttr.Value)) return false;

            var checkIntervalNode = githubRepoNode.SelectSingleNode("checkIntervalMinutes");
            if (checkIntervalNode == null) return false;
            if (string.IsNullOrEmpty(checkIntervalNode.InnerText)) return false;
            if (!uint.TryParse(checkIntervalNode.InnerText, out uint interval)) return false;

            var lastReleaseDateNode = githubRepoNode.SelectSingleNode("lastDeployedReleaseDate");
            if (lastReleaseDateNode != null)
            {
                if (string.IsNullOrEmpty(lastReleaseDateNode.InnerText)) return false;
                if (!DateTime.TryParse(lastReleaseDateNode.InnerText, out DateTime date)) return false;
            }

            return true;
        }

        public uint GetCheckInterval()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo/checkIntervalMinutes");
            return uint.Parse(node.InnerText);
        }

        public string GetDeploySite()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo/deploy");
            return node.InnerText;
        }

        public string GetDeployType()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo/deploy");
            return node.Attributes.GetNamedItem("type").Value;
        }

        public string GetGitRepo()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo");
            return node.Attributes.GetNamedItem("repo").Value;
        }

        public string GetGitToken()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo/token");
            return node.InnerText;
        }

        public string GetGitUserName()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo");
            return node.Attributes.GetNamedItem("username").Value;
        }

        public DateTime? GetLastDeployedReleaseDate()
        {
            var node = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo/lastDeployedReleaseDate");
            if (node == null) return null;
            return DateTime.ParseExact(node.InnerText, "yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo);
        }

        public bool SetLastDeployedReleaseDate(string releaseDate)
        {
            try
            {
                var date = DateTime.ParseExact(releaseDate, "yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo);
                var existingDate = GetLastDeployedReleaseDate();

                if (existingDate != null && existingDate >= date)
                {
                    return false;
                }

                var repoNode = _doc.DocumentElement.SelectSingleNode("/serviceConfig/gitRepo");
                XmlElement lastReleaseDateNode = _doc.CreateElement("lastDeployedReleaseDate");
                lastReleaseDateNode.InnerText = date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo);
                var existingNode = repoNode.SelectSingleNode("/serviceConfig/gitRepo/lastDeployedReleaseDate");
                if (existingNode != null) repoNode.RemoveChild(existingNode);
                repoNode.AppendChild(lastReleaseDateNode);
                _doc.Save(_filePath);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
