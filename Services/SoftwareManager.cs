using System;
using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class SoftwareManager(IServiceProvider serviceProvider) : ISoftwareManager
{
    public IEnumerable<ISoftwareService> GetAllSoftwares()
    {
        return serviceProvider.GetServices<ISoftwareService>();
    }

    public ISoftwareService GetSoftwares(string identifier)
    {
        return GetAllSoftwares().FirstOrDefault(x => x.Identifier == identifier);
    }
}