using log4net;
using ReleaseDeployerService.Core;
using System.Composition;

namespace ReleaseDeployerService
{
    [Export(typeof(ILogFactory))]
    public class MefLogFactory : ILogFactory
    {
        public ILog Create(Type type)
        {
            return LogManager.GetLogger(type);
        }
    }
}
