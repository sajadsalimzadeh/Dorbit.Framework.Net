namespace Dorbit.Services.Abstractions;

public interface IMonitorService
{
    double AvgResponseTimeMs { get; }
    int AvgResponseItemCount { get; set; }
    int RequestPerSecond { get; }

    void AddRequest();
    void AddResponseDuration(int milliseconds);
}