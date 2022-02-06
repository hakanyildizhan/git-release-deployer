using System.Composition.Convention;
using System.Composition.Hosting;
using System.Runtime.Loader;

namespace ReleaseDeployerService
{
    static class ContainerConfigurationExtensions
    {
        public static ContainerConfiguration WithExport<T>(this ContainerConfiguration configuration, T exportedInstance, string contractName = null, IDictionary<string, object> metadata = null)
        {
            return WithExport(configuration, exportedInstance, typeof(T), contractName, metadata);
        }

        public static ContainerConfiguration WithExport(this ContainerConfiguration configuration, object exportedInstance, Type contractType, string contractName = null, IDictionary<string, object> metadata = null)
        {
            return configuration.WithProvider(new InstanceExportDescriptorProvider(
                exportedInstance, contractType, contractName, metadata));
        }

        public static ContainerConfiguration WithAssembliesInPath(this ContainerConfiguration configuration, string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return WithAssembliesInPath(configuration, path, null, searchOption);
        }

        public static ContainerConfiguration WithAssembliesInPath(this ContainerConfiguration configuration, string path, AttributedModelProvider conventions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var assemblyFiles = Directory.GetFiles(path, "*.dll", searchOption);
            var assemblies = assemblyFiles.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);
            configuration = configuration.WithAssemblies(assemblies, conventions);
            return configuration;
        }
    }
}
