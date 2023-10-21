using Dorbit.Models;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Dorbit.Installers;

public static class PrometheusInstaller
{
    public static KestrelMetricServer StartPrometheusServer(this IServiceCollection services, Action<PrometheusConfig> registeration)
    {
        var config = new PrometheusConfig();
        registeration(config);
        var server = new KestrelMetricServer(port: config.Port);
        server.Start();
        return server;
    }
}