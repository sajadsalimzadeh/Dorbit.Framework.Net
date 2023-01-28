using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Devor.Framework.Hosts
{
    public abstract class BaseHost
    {
        private readonly IServiceProvider serviceProvider;
        protected readonly IThreadService threadService;

        protected Thread Thread { get; set; }
        protected abstract int IntervalSeconds { get; }

        public BaseHost(IServiceProvider serviceProvider)
        {
            threadService = serviceProvider.GetService<IThreadService>();
            this.serviceProvider = serviceProvider;
        }

        public void Start()
        {
            Thread = new Thread(() =>
            {
                for (long i = 0; threadService.MainThread.IsAlive; i++)
                {
                    try
                    {
                        if (i % IntervalSeconds != 0) continue;
                        var sp = serviceProvider.CreateScope().ServiceProvider;
                        var loggerService = sp.GetService<ILoggerService>();
                        try
                        {
                            Invoke(sp);
                        }
                        catch (Exception ex)
                        {
                            loggerService.LogError(ex);
                        }
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            });
            Thread.Start();
        }

        protected abstract void Invoke(IServiceProvider serviceProvider);
    }
}
