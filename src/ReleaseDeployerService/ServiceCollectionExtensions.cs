using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;

namespace ReleaseDeployerService
{
    public static class ServiceCollectionExtensions 
    { 
        public static IServiceCollection AddFromAssembliesInPath<T>(this IServiceCollection services, ServiceLifetime lifetime, string path = null) where T : class 
        { 
            var factory = new ExportFactory<T, object>(() => new Tuple<T, Action>(Activator.CreateInstance<T>(), () => { }), new object()); 
            var conventions = new ConventionBuilder(); 
            var builder = conventions.ForTypesDerivedFrom<T>().Export<T>(); 
            
            if (lifetime == ServiceLifetime.Singleton) 
            { 
                builder = builder.Shared(); 
            }
            
            path = path ?? AppContext.BaseDirectory;
            var configuration = new ContainerConfiguration()
                .WithAssembliesInPath(path, conventions);

            using (var container = configuration.CreateContainer()) 
            {
                var svcs = container.GetExports<Lazy<T>>(); 
                
                foreach (var svc in svcs) 
                { 
                    services.Add(new ServiceDescriptor(typeof(T), sp => svc.Value, lifetime)); 
                } 
            } return services; 
        } 
        
        public static IServiceCollection AddSingletonFromAssembliesInPath<T>(this IServiceCollection services, string path = null) where T : class 
        { 
            return AddFromAssembliesInPath<T>(services, ServiceLifetime.Singleton, path); 
        } 
        
        public static IServiceCollection AddScopedFromAssembliesInPath<T>(this IServiceCollection services, string path = null) where T : class 
        { 
            return AddFromAssembliesInPath<T>(services, ServiceLifetime.Scoped, path); 
        } 
        
        public static IServiceCollection AddTransientFromAssembliesInPath<T>(this IServiceCollection services, string path = null) where T : class 
        { 
            return AddFromAssembliesInPath<T>(services, ServiceLifetime.Transient, path); 
        } 
    }
}
