using Devor.Framework.Attributes;
using Devor.Framework.Services.Abstractions;
using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Devor.Framework.Services
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class ThreadService : IThreadService
    {
        static Thread mainThread;
        public Thread MainThread { get { return mainThread; } set { mainThread = value; } }
    }
}
