using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Installers;

public static class LetsEncryptInstaller
{
    public static IServiceCollection AddDorbitLetsEncrypt(this IServiceCollection services)
    {
        services.AddLettuceEncrypt();
        return services;
    }
}