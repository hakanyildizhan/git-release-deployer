using log4net;

namespace ReleaseDeployerService.Core
{
    public interface ILogFactory
    {
        ILog Create(Type type);
    }
}
