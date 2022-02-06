using ReleaseDeployerService.Core;
using System.Composition;

namespace ReleaseDeployerService
{
    /// <summary>
    /// Wrapper for type <see cref="XmlConfigReader"/> in order to expose it through MEF.
    /// </summary>
    [Export(typeof(IConfigReader))]
    public class XmlConfigReaderMEF : XmlConfigReader
    {
        public XmlConfigReaderMEF() : base(ServiceConfiguration.LOG_PATH) { }
    }
}
