using Devor.Framework.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Devor.Framework.Extensions
{
    public static class AttributeRegisterarExtensions
    {

        public static Assembly[] WhereByNamespace(this Assembly[] assemblies, string _namespace)
        {
            var list = assemblies.ToList();
            list = list.Where(x => x.GetTypes().FirstOrDefault()?.Namespace?.IndexOf(_namespace) > -1).ToList();
            return list.ToArray();
        }

        public static List<Assembly> GetAssemblies(Assembly entryAssembly, string[] namespaces)
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
                    if (!loadedAssemblies.Contains(reference.FullName))
                    {
                        try
                        {
                            var assembly = Assembly.Load(reference);
                            assembliesToCheck.Enqueue(assembly);
                        }
                        catch
                        {

                        }
                        loadedAssemblies.Add(reference.FullName);
                    }
                }

                if (namespaces.Any() || namespaces.Any(x => assemblyToCheck.FullName.StartsWith(x)))
                {
                    returnAssemblies.Add(assemblyToCheck);
                }
            }
            return returnAssemblies;
        }

        internal static Type[] GetInterfacesDirect(this Type type)
        {
            var allInterfaces = new List<Type>();
            var childInterfaces = new List<Type>();

            foreach (var item in type.GetInterfaces())
            {
                allInterfaces.Add(item);
                foreach (var item2 in item.GetInterfaces())
                    childInterfaces.Add(item2);
            }
            return allInterfaces.Except(childInterfaces).ToArray();
        }

        internal static void RegisterServicesByAttribute(this IServiceCollection services, Assembly entryAssembly)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var namespaces = new string[] { "Devor", "IME" };
            var assemblies = GetAssemblies(entryAssembly, namespaces);
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var registerAttr = type.GetCustomAttribute<ServiceRegisterarAttribute>();
                        if (registerAttr is null) continue;
                        services.RegisterServicesByAttribute(type, type, registerAttr);
                    }
                }
                catch { }
            }
        }

        internal static void RegisterServicesByAttribute(this IServiceCollection services, Type implementationType, Type[] types, ServiceRegisterarAttribute attr)
        {
            foreach (var type in types)
            {
                services.RegisterServicesByAttribute(type, implementationType, attr);
            }
        }

        internal static void RegisterServicesByAttribute(this IServiceCollection services, Type serviceType, Type implementationType, ServiceRegisterarAttribute registerAttr)
        {
            if (registerAttr.ImplementationFactoryResolver is not null)
            {
                Func<IServiceProvider, object> implementationFactory = (sp) =>
                {
                    var resolver = sp.GetService(registerAttr.ImplementationFactoryResolver);
                    if (resolver is null) return default;
                    var resolveMethod = resolver.GetType().GetMethod("Resolve");
                    if (resolveMethod is null) return default;
                    var instance = resolveMethod.Invoke(resolver, new object[] { });
                    return instance;
                };
                services.Add(new ServiceDescriptor(serviceType, implementationFactory, registerAttr.Lifetime));
            }
            else if (implementationType.IsInterface)
            {
                services.Add(new ServiceDescriptor(serviceType, sp => sp.GetService(implementationType), registerAttr.Lifetime));
            }
            else if (!implementationType.IsAbstract)
            {
                services.Add(new ServiceDescriptor(serviceType, implementationType, registerAttr.Lifetime));
            }
            services.RegisterServicesByAttribute(serviceType, serviceType.GetInterfacesDirect(), registerAttr);
        }
    }
}
