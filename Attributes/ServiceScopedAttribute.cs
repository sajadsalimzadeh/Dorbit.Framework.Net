using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Attributes;

public class ServiceScopedAttribute : ServiceRegisterAttribute
{
    public ServiceScopedAttribute()
    {
        Lifetime = ServiceLifetime.Scoped;
    }
}