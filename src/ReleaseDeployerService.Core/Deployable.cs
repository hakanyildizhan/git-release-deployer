using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseDeployerService.Core
{
    public struct Deployable
    {
        public string Path { get; set; }
        public DeployableType DeployableType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
