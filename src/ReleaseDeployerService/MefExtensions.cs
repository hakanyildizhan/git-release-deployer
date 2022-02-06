using System.Composition.Convention;
using System.Composition.Hosting;
using System.Runtime.Loader;

namespace ReleaseDeployerService
{
    public static class MefExtensions
    {
        public static IServiceCollection AddMefService<T>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            var conventions = new ConventionBuilder();
            var builder = conventions.ForTypesDerivedFrom<T>().Export<T>();

            if (lifetime == ServiceLifetime.Singleton)
            {
                builder = builder.Shared();
            }

            var assemblyFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            var assemblies = assemblyFiles.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies, conventions);

            using (var container = configuration.CreateContainer())
            {
                var svcs = container.GetExports<Lazy<T>>();
                foreach (var svc in svcs)
                {
                    services.Add(new ServiceDescriptor(typeof(T), sp => svc.Value, lifetime));
                }
            }

            return services;
        }
    }
}
