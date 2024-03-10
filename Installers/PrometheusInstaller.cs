using System;
using Dorbit.Framework.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Dorbit.Framework.Installers;

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