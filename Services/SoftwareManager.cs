using Dorbit.Attributes;
using Dorbit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class SoftwareManager : ISoftwareManager
{
    private readonly IServiceProvider _serviceProvider;

    public SoftwareManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<ISoftwareService> GetAllSoftwares()
    {
        return _serviceProvider.GetServices<ISoftwareService>();
    }

    public ISoftwareService GetSoftwares(string identifier)
    {
        return GetAllSoftwares().FirstOrDefault(x => x.Identifier == identifier);
    }
}