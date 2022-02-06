using log4net;
using ReleaseDeployerService.Core;
//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.log4net;

namespace ReleaseDeployerService
{
    public sealed class UnityServiceProvider : IUnityServiceProvider
    {
        private static readonly Lazy<IUnityServiceProvider> lazy = new Lazy<IUnityServiceProvider>(() => new UnityServiceProvider());
        public static IUnityServiceProvider Instance { get { return lazy.Value; } }

        private IUnityContainer _container;

        //[Import]
        //public IDeployer _deployer { get; set; }

        private UnityServiceProvider()
        {
            _container = BuildUnityContainer();
#if DEBUG
            _container.AddExtension(new Diagnostic());
#endif
        }

        /// <summary>Gets the service object of the specified type.</summary>
        /// <returns>A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.</returns>
        /// <param name="serviceType">An object that specifies the type of service object to get. </param>
        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns></returns>
        public object GetService<T>()
        {
            return _container.Resolve<T>();
        }

        private IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            container.AddNewExtension<Log4NetExtension>();
            container.RegisterSingleton<IConfigReader, XmlConfigReader>(new InjectionConstructor(ServiceConfiguration.LOG_PATH));
            container.RegisterSingleton<IAssetDownloader, GithubV3AssetDownloader>();

            //var catalog = new DirectoryCatalog(".");
            //using (var compContainer = new CompositionContainer(catalog))
            //{
            //    compContainer.ComposeParts(this);
            //}
            File.AppendAllText(@"C:\Users\yildi\Downloads\logg.txt", "0\r\n");
            try
            {
                var assemblies = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.dll", SearchOption.AllDirectories)
                    .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                    .ToList();
                var containerConfig = new ContainerConfiguration().WithAssemblies(assemblies).WithExport(container.Resolve<ILog>()).WithExport(container.Resolve<IConfigReader>());
                using (var pluginContainer = containerConfig.CreateContainer())
                {
                    //pluginContainer.SatisfyImports(this);
                    File.AppendAllText(@"C:\Users\yildi\Downloads\logg.txt", "1\r\n");
                    var exp = pluginContainer.GetExport<IDeployer>();
                    File.AppendAllText(@"C:\Users\yildi\Downloads\logg.txt", $"2: {exp != null}\r\n");
                    container.RegisterInstance<IDeployer>(exp);
                    File.AppendAllText(@"C:\Users\yildi\Downloads\logg.txt", "3\r\n");
                }


                //container.RegisterInstance<IDeployer>(_deployer);
            }
            catch (Exception ex)
            {
                //container.Resolve<ILog>().Error("Error: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            
            return container;
        }
    }
}
