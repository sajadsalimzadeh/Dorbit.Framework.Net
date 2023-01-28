using Devor.Framework.Attributes;
using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devor.Framework.Services
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
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
}
