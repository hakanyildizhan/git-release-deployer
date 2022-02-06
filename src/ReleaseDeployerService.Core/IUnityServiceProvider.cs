using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseDeployerService.Core
{
    public interface IUnityServiceProvider : IServiceProvider
    {
        object GetService<T>();
    }
}
