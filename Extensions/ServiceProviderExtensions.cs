using System;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Repositories.Abstractions;

namespace Dorbit.Framework.Extensions;

public static class ServiceProviderExtensions
{
    public static ReflectedRepository GetReflectedRepository(this IServiceProvider serviceProvider, Type entityType)
    {
        var genericType = typeof(IBaseRepository<,>).MakeGenericType(entityType, typeof(Guid));
        var methodInfo = serviceProvider.GetType().GetMethod("GetService") ?? throw new Exception("Method GetService() not found");
        var repository = methodInfo.Invoke(serviceProvider, [genericType]);
        return new ReflectedRepository(repository);
    }
}