﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dorbit.Framework.Extensions;

public static class ServiceCollectionExtensions
{
    private static IMvcBuilder _mvcBuilder;

    private class Descriptor
    {
        public int Order { get; set; }
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }

    public static void RegisterServicesByAssembly(this IServiceCollection services, Assembly entryAssembly, string[] prefixes)
    {
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies(prefixes);
        var descriptors = new List<Descriptor>();
        foreach (var assembly in allAssemblies)
        {
            services.AddControllers(assembly);
            try
            {
                foreach (var serviceType in assembly.GetTypes())
                {
                    var registerAttr = serviceType.GetCustomAttribute<ServiceRegisterAttribute>();
                    if (registerAttr is null) continue;
                    services.RegisterServicesByAttribute(serviceType, serviceType, registerAttr, descriptors, registerAttr.RecursiveLevelCount);
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
        List<Descriptor> descriptors, int level)
    {
        if (serviceType.IsInterface)
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

        if (registerAttr.Recursive && level >= 0)
        {
            foreach (var type in serviceType.GetInterfacesDirect())
            {
                services.RegisterServicesByAttribute(type, implementationType, registerAttr, descriptors, level - 1);
            }
        }
    }

    public static T BindConfiguration<T>(this IServiceCollection services, params string[] additionalEnvironments) where T : class
    {
        var environment = EnvironmentUtil.GetEnvironment();
        Console.WriteLine($"Bind Configuration '{typeof(T).Name}' With Environment: {environment} {Path.Combine(AppDomain.CurrentDomain.BaseDirectory)}");
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.BuildDorbit();

        foreach (var additionalEnvironment in additionalEnvironments)
        {
            configurationBuilder.AddJsonFile($"appsettings.{additionalEnvironment}.json", true, true);
        }

        var settings = configurationBuilder.Build();
        var appSettings = Activator.CreateInstance<T>();
        settings.Bind(appSettings);
        services.Configure<T>(settings);
        services.AddSingleton(appSettings);
        return appSettings;
    }

    public static IMvcBuilder AddControllers(this IServiceCollection services, Assembly assembly)
    {
        if (_mvcBuilder is null)
        {
            _mvcBuilder = services.AddControllers();
            services.TryAddSingleton(_mvcBuilder);
        }

        _mvcBuilder.AddApplicationPart(assembly);
        return _mvcBuilder;
    }

    public static IMvcBuilder AddControllers<T>(this IServiceCollection services)
    {
        return services.AddControllers(typeof(T).Assembly);
    }
}