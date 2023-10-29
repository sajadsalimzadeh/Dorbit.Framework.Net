using System.Reflection;
using Dorbit.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Extensions;

public static class ServiceCollectionExtensions
{
    private class Descriptor
    {
        public int Order { get; set; }
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }

    public static void RegisterServicesByAssembly(this IServiceCollection services, string[] namespaces)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var assemblies = GetAssemblies(entryAssembly, namespaces);
        var descriptors = new List<Descriptor>();
        foreach (var assembly in assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    var registerAttr = type.GetCustomAttribute<ServiceRegisterAttribute>();
                    if (registerAttr is null) continue;
                    services.RegisterServicesByAttribute(type, type, registerAttr, descriptors);
                }
            }
            catch
            {
                // ignored
            }
        }

        foreach (var descriptor in descriptors.OrderBy(x => x.Order))
        {
            services.Add(descriptor.ServiceDescriptor);
        }
    }

    private static List<Assembly> GetAssemblies(Assembly entryAssembly, string[] namespaces)
    {
        var returnAssemblies = new List<Assembly>();
        var loadedAssemblies = new List<string>();
        var assembliesToCheck = new Queue<Assembly>();
        assembliesToCheck.Enqueue(entryAssembly);
        while (assembliesToCheck.Any())
        {
            var assemblyToCheck = assembliesToCheck.Dequeue();
            foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
            {
                if (loadedAssemblies.Contains(reference.FullName)) continue;
                try
                {
                    var assembly = Assembly.Load(reference);
                    assembliesToCheck.Enqueue(assembly);
                }
                catch
                {
                    // ignored
                }

                loadedAssemblies.Add(reference.FullName);
            }

            if (namespaces.Any(x => assemblyToCheck.FullName != null && assemblyToCheck.FullName.StartsWith(x)))
            {
                returnAssemblies.Add(assemblyToCheck);
            }
        }

        return returnAssemblies;
    }

    private static IEnumerable<Type> GetInterfacesDirect(this Type type)
    {
        var allInterfaces = new List<Type>();
        var childInterfaces = new List<Type>();

        foreach (var item in type.GetInterfaces())
        {
            allInterfaces.Add(item);
            childInterfaces.AddRange(item.GetInterfaces());
        }

        return allInterfaces.Except(childInterfaces).ToArray();
    }

    private static void RegisterServicesByAttribute(this IServiceCollection services,
        Type serviceType, Type implementationType,
        ServiceRegisterAttribute registerAttr,
        List<Descriptor> descriptors)
    {
        if (implementationType.IsInterface)
        {
            descriptors.Add(new Descriptor()
            {
                Order = registerAttr.Order,
                ServiceDescriptor = new ServiceDescriptor(serviceType, sp => sp.GetService(implementationType), registerAttr.Lifetime)
            });
        }
        else if (!implementationType.IsAbstract)
        {
            descriptors.Add(new Descriptor()
            {
                Order = registerAttr.Order,
                ServiceDescriptor = new ServiceDescriptor(serviceType, implementationType, registerAttr.Lifetime)
            });
        }

        foreach (var type in serviceType.GetInterfacesDirect())
        {
            services.RegisterServicesByAttribute(type, implementationType, registerAttr, descriptors);
        }
    }

    private static string GetAppSettingsFilename()
    {
        var names = new List<string>();
        names.Add($"appsettings.{AppDomain.CurrentDomain.GetEnvironment()?.ToLower()}.json");
        names.Add("appsettings.json");
        foreach (var name in names)
        {
            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(filename)) return filename;
        }

        throw new Exception("AppSettings file not found");
    }

    public static T BindConfiguration<T>(this IServiceCollection services, string filename = null) where T : class
    {
        if (string.IsNullOrEmpty(filename))
        {
            filename = GetAppSettingsFilename();
        }

        var basePath = Directory.GetParent(AppContext.BaseDirectory)?.FullName ?? "./";
        var settings = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(filename, false).Build();

        var appSettings = Activator.CreateInstance<T>();
        settings.Bind(appSettings);
        services.AddSingleton(appSettings);
        return appSettings;
    }
}