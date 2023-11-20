using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Services.Abstractions;

public interface IConfigurationLogger
{
    void Configure(IServiceCollection services);
}