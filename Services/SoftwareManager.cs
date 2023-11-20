using Dorbit.Attributes;
using Dorbit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class SoftwareManager : ISoftwareManager
{
    private readonly IServiceProvider serviceProvider;

    public SoftwareManager(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IEnumerable<ISoftwareService> GetAllSoftwares()
    {
        return serviceProvider.GetServices<ISoftwareService>();
    }

    public ISoftwareService GetSoftwares(string identifier)
    {
        return GetAllSoftwares().FirstOrDefault(x => x.Identifier == identifier);
    }
}