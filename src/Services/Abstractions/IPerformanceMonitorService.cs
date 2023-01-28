using Devor.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devor.Framework.Services.Abstractions
{
    public interface IPerformanceMonitorService
    {
        float CpuUsage { get; }
        long TotalVisibleMemorySize { get; }
        long FreePhysicalMemory { get; }
        long TotalVirtualMemorySize { get; }
        long FreeVirtualMemory { get; }
        Dictionary<string, NetworkInfo> Networks { get; }

        IPerformanceMonitorService StartMemoryMonitoring();
        IPerformanceMonitorService StartNetworkMonitoring();
        IPerformanceMonitorService StartProcessorMonitoring();
    }
}
