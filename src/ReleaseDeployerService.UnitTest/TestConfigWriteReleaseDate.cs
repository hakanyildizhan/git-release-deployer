using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReleaseDeployerService.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseDeployerService.UnitTest
{
    [TestClass]
    public class TestConfigWriteReleaseDate
    {
        [TestMethod]
        [DataRow("2022-01-30T16:03:57Z")]
        public void Write_Release_Date_To_Config_File_With_No_Release_Date_Config_Is_Valid(string releaseDate)
        {
            var date = DateTime.ParseExact(releaseDate, "yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
            string filePath = Path.Combine("ConfigFiles", "testconfig1.xml");
            string newFilePath = "testconfig1_copy.xml";
            File.Copy(filePath, newFilePath, true);
            IConfigReader configReader = new XmlConfigReader(newFilePath);
            var lastDeployDate = configReader.GetLastDeployedReleaseDate();
            Assert.IsNull(lastDeployDate);
            Assert.IsTrue(configReader.SetLastDeployedReleaseDate(date));
            Assert.IsTrue(configReader.CheckValidity());
            Assert.AreEqual(configReader.GetLastDeployedReleaseDate(), date);
        }

        [TestMethod]
        [DataRow("2022-01-31T12:33:57Z")]
        public void Update_Release_Date_With_Newer_Date_Config_Is_Valid(string releaseDate)
        {
            string filePath = Path.Combine("ConfigFiles", "testconfig2.xml");
            string newFilePath = "testconfig2_copy1.xml";
            File.Copy(filePath, newFilePath, true);
            IConfigReader configReader = new XmlConfigReader(newFilePath);
            var lastDeployDate = configReader.GetLastDeployedReleaseDate();
            var newerReleaseDate = DateTime.ParseExact(releaseDate, "yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
            Assert.IsTrue(lastDeployDate < newerReleaseDate);
            Assert.IsTrue(configReader.SetLastDeployedReleaseDate(newerReleaseDate));
            Assert.IsTrue(configReader.CheckValidity());
            Assert.AreEqual(configReader.GetLastDeployedReleaseDate(), newerReleaseDate);
        }

        [TestMethod]
        public void Update_Release_Date_With_Same_Date_Do_Nothing()
        {
            string filePath = Path.Combine("ConfigFiles", "testconfig2.xml");
            string newFilePath = "testconfig2_copy2.xml";
            File.Copy(filePath, newFilePath, true);
            IConfigReader configReader = new XmlConfigReader(newFilePath);
            var existingDate = configReader.GetLastDeployedReleaseDate();
            Assert.IsFalse(configReader.SetLastDeployedReleaseDate(existingDate.Value));
            Assert.AreEqual(configReader.GetLastDeployedReleaseDate(), DateTime.ParseExact("2022-01-30T16:03:57Z", "yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal));
            Assert.IsTrue(configReader.CheckValidity());
        }

        [TestMethod]
        [DataRow("2022-01-01T12:33:57Z")]
        public void Update_Release_Date_With_Older_Date_Do_Nothing(string olderDate)
        {
            var date = DateTime.ParseExact(olderDate, "yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
            string filePath = Path.Combine("ConfigFiles", "testconfig2.xml");
            string newFilePath = "testconfig2_copy2.xml";
            File.Copy(filePath, newFilePath, true);
            IConfigReader configReader = new XmlConfigReader(newFilePath);
            Assert.IsFalse(configReader.SetLastDeployedReleaseDate(date));
            Assert.AreNotEqual(configReader.GetLastDeployedReleaseDate(), date);
            Assert.IsTrue(configReader.CheckValidity());
        }
    }
}
