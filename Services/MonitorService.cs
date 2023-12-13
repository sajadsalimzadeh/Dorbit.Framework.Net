using Dorbit.Attributes;
using Dorbit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class MonitorService : IMonitorService
{
    private int _responseDurationIndex = 0;
    private Dictionary<int, int> _responseDurations = new Dictionary<int, int>();

    public int AvgResponseItemCount { get; set; } = 100;
    public double AvgResponseTimeMs => (_responseDurations.Count > 0 ? _responseDurations.Values.Average() : 0);

    private DateTime _requestTime = DateTime.Now;
    public int RequestPerSecond { get; private set; }

    public void AddResponseDuration(int milliseconds)
    {
        lock (_responseDurations)
        {
            _responseDurations[_responseDurationIndex++] = milliseconds;
            if (_responseDurationIndex >= AvgResponseItemCount) _responseDurationIndex = 0;
        }
    }

    public void AddRequest()
    {
        if((DateTime.Now - _requestTime).Seconds > 0)
        {
            RequestPerSecond = 0;
            _requestTime = DateTime.Now;
        }
        RequestPerSecond++;
    }
}