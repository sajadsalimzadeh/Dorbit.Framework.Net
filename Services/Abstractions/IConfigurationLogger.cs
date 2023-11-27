using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services.Abstractions;

public interface IConfigurationLogger
{
    void Configure(IServiceCollection services);
}