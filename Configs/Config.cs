using Dorbit.Framework.Configs.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Configs;

public class Config<TOptions> : IConfig<TOptions> where TOptions : class
{
    public IConfiguration Configuration { get; }

    public Config(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void Configure(IServiceCollection services)
    {
        services.Configure<TOptions>(Configuration);
    }
}
