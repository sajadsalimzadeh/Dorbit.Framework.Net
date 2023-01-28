using Microsoft.Extensions.DependencyInjection;

namespace Devor.Framework.Services.Abstractions
{
    public interface IConfigurationLogger
    {
        void Configure(IServiceCollection services);
    }
}
