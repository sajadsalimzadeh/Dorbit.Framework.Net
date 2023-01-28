#pragma warning disable CA1416 // Validate platform compatibility
using Devor.Framework.Attributes;
using Devor.Framework.Models;
using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace Devor.Framework.Services
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class PerformanceMonitorService : IPerformanceMonitorService
    {
        private Thread processorThread;
        private Thread memoryThread;
        private Thread networkThread;
        private readonly IThreadService threadService;

        public float CpuUsage { get; private set; }
        public long TotalVisibleMemorySize { get; private set; }
        public long FreePhysicalMemory { get; private set; }
        public long TotalVirtualMemorySize { get; private set; }
        public long FreeVirtualMemory { get; private set; }

        public Dictionary<string, NetworkInfo> Networks { get; } = new Dictionary<string, NetworkInfo>();

        public PerformanceMonitorService(IThreadService threadService)
        {
            this.threadService = threadService;
        }

        private IEnumerable<string> GetNetworkCards()
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
            return category.GetInstanceNames();
        }
        private NetworkInfo GetNetworkInfo(string networkCard)
        {
            if (!Networks.TryGetValue(networkCard, out var network)) Networks[networkCard] = network = new NetworkInfo(networkCard);
            return network;
        }

        public IPerformanceMonitorService StartProcessorMonitoring()
        {
            if (processorThread is null)
            {
                processorThread = new Thread(() =>
                {
                    var counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    while (threadService.MainThread?.IsAlive != false)
                    {
                        Thread.Sleep(1000);
                        CpuUsage = counter.NextValue();
                    }
                });
                processorThread.Start();
            }
            return this;
        }
        public IPerformanceMonitorService StartMemoryMonitoring()
        {
            if (memoryThread is null)
            {
                memoryThread = new Thread(() =>
                {
                    while (threadService.MainThread?.IsAlive != false)
                    {
                        Thread.Sleep(1000);
                        ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                        ManagementObjectCollection results = searcher.Get();

                        foreach (ManagementObject result in results)
                        {
                            TotalVisibleMemorySize = Convert.ToInt64(result["TotalVisibleMemorySize"]);
                            FreePhysicalMemory = Convert.ToInt64(result["FreePhysicalMemory"]);
                            TotalVirtualMemorySize = Convert.ToInt64(result["TotalVirtualMemorySize"]);
                            FreeVirtualMemory = Convert.ToInt64(result["FreeVirtualMemory"]);
                        }
                    }
                });
                memoryThread.Start();
            }
            return this;
        }
        public IPerformanceMonitorService StartNetworkMonitoring()
        {
            if (networkThread is null)
            {
                networkThread = new Thread(() =>
                {
                    while (threadService.MainThread?.IsAlive != false)
                    {
                        foreach (var networkCard in GetNetworkCards())
                        {
                            var network = GetNetworkInfo(networkCard);

                            var bandWidth = network.BandwidthCounter.NextValue();
                            var sendByte = network.DataSentCounter.NextValue();
                            var receiveByte = network.DataReceivedCounter.NextValue();
                            if (bandWidth > 0) network.BandWidth = bandWidth;
                            if (sendByte > 0) network.SendByte = sendByte;
                            if (receiveByte > 0) network.ReceiveByte = receiveByte;
                        }
                        Thread.Sleep(100);
                    }
                });
                networkThread.Start();
            }
            return this;
        }

    }
}
