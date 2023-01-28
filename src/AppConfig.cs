using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Devor.Framework
{
    public class AppConfigSeq : IConfigurationLogger
    {
        public string ServerUrl { get; set; }
        public string ApiKey { get; set; }
        public LogEventLevel Level { get; set; } = LogEventLevel.Information;

        public AppConfigSeq(string serverUrl = "http://localhost:5341", string apiKey = null)
        {
            ServerUrl = serverUrl;
            ApiKey = apiKey;
        }

        public void Configure(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                     .WriteTo.Seq(ServerUrl, apiKey: ApiKey, restrictedToMinimumLevel: Level)
                     .CreateLogger();

            services.AddSingleton(Log.Logger);
        }
    }
}
