using Devor.Framework.Attributes;
using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Devor.Framework.Hosts
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class StartupHost : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public StartupHost(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var serviceProvider = this.serviceProvider.CreateScope().ServiceProvider;
            var threadService = serviceProvider.GetService<IThreadService>();
            threadService.MainThread = Thread.CurrentThread;
            var logger = serviceProvider.GetService<ILogger<IStartup>>();
            var startups = serviceProvider.GetServices<IStartup>();
            foreach (var startup in startups)
            {
                try { startup.Run(); }
                catch (Exception ex) { logger?.LogError(ex, "Run Startup Failed"); }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
