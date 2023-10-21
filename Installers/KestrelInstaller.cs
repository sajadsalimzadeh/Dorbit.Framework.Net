using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Dorbit.Installers;

public static class KestrelInstaller
{
    public static WebApplicationBuilder SecureKestrel(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel(config => { config.AddServerHeader = false; });
        return builder;
    }
}