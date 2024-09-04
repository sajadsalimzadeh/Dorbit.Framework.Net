using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Configs.Abstractions;

public interface IConfig<T>
{
    IConfiguration Configuration { get; }

    void Configure(IServiceCollection services);
}