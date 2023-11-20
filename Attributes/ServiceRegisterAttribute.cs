using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceRegisterAttribute : Attribute
{
    public int Order { get; set; } = 0;
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}