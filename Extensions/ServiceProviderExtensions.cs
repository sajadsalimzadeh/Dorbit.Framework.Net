using System;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Repositories.Abstractions;

namespace Dorbit.Framework.Extensions;

public static class ServiceProviderExtensions
{
    public static ReflectedRepository GetReflectedRepository(this IServiceProvider serviceProvider, Type entityType)
    {
        var genericType = typeof(IBaseRepository<,>).MakeGenericType(entityType, typeof(Guid));
        return new ReflectedRepository(serviceProvider.GetService(genericType));
    }
}