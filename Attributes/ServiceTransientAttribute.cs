using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Attributes;

public class ServiceTransientAttribute : ServiceRegisterAttribute
{
    public ServiceTransientAttribute()
    {
        Lifetime = ServiceLifetime.Transient;
    }
}