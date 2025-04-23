using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Dorbit.Framework.Installers.Configurations;

public class AppConfigSeq(string serverUrl = "http://localhost:5341", string apiKey = null) : IConfigurationLogger
{
    public string ServerUrl { get; set; } = serverUrl;
    public string ApiKey { get; set; } = apiKey;
    public LogEventLevel Level { get; set; } = LogEventLevel.Information;

    public void Configure(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Seq(ServerUrl, apiKey: ApiKey, restrictedToMinimumLevel: Level)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
    }
}