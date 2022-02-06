using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReleaseDeployerService.Core;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ReleaseDeployerService.UnitTest
{
    [TestClass]
    public class TestConfigReading
    {
        [TestMethod]
        public void Valid_Config_Without_LastReleaseDate_Is_Read_Correctly()
        {
            IConfigReader configReader = new XmlConfigReader("ConfigFiles\\testconfig1.xml");
            Assert.IsTrue(configReader.CheckValidity());
        }

        [TestMethod]
        public void Valid_Config_With_LastReleaseDate_Is_Read_Correctly()
        {
            IConfigReader configReader = new XmlConfigReader("ConfigFiles\\testconfig2.xml");
            Assert.IsTrue(configReader.CheckValidity());
        }

        [TestMethod]
        public void Config_Missing_ServiceConfig_Node_Cannot_Be_Read()
        {
            IConfigReader configReader = new XmlConfigReader("ConfigFiles\\testconfig3.xml");
            Assert.IsFalse(configReader.CheckValidity());
        }

        [TestMethod]
        public void Config_Missing_Deploy_Type_Cannot_Be_Read()
        {
            IConfigReader configReader = new XmlConfigReader("ConfigFiles\\testconfig4.xml");
            Assert.IsFalse(configReader.CheckValidity());
        }

        [TestMethod]
        public void Config_Incorrect_Interval_Cannot_Be_Read()
        {
            IConfigReader configReader = new XmlConfigReader("ConfigFiles\\testconfig5.xml");
            Assert.IsFalse(configReader.CheckValidity());
        }
    }
}