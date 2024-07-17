using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Attributes;

public class ServiceSingletoneAttribute : ServiceRegisterAttribute
{
    public ServiceSingletoneAttribute()
    {
        Lifetime = ServiceLifetime.Singleton;
    }
}