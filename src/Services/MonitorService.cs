using Devor.Framework.Attributes;
using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devor.Framework.Services
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class MonitorService : IMonitorService
    {
        private int responseDurationIndex = 0;
        private Dictionary<int, int> responseDurations = new Dictionary<int, int>();

        public int AvgResponseItemCount { get; set; } = 100;
        public double AvgResponseTimeMs => (responseDurations.Count > 0 ? responseDurations.Values.Average() : 0);

        private DateTime requestTime = DateTime.Now;
        public int RequestPerSecond { get; private set; }

        public void AddResponseDuration(int milliseconds)
        {
            lock (responseDurations)
            {
                responseDurations[responseDurationIndex++] = milliseconds;
                if (responseDurationIndex >= AvgResponseItemCount) responseDurationIndex = 0;
            }
        }

        public void AddRequest()
        {
            if((DateTime.Now - requestTime).Seconds > 0)
            {
                RequestPerSecond = 0;
                requestTime = DateTime.Now;
            }
            RequestPerSecond++;
        }
    }
}
