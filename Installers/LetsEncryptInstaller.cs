using System.IO;
using LettuceEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Installers;

public static class LetsEncryptInstaller
{
    public static IServiceCollection AddDorbitLetsEncrypt(this IServiceCollection services)
    {
        var path = Path.Combine("../Certificates/");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        services.AddLettuceEncrypt()
            .PersistDataToDirectory(new DirectoryInfo(path), "123456");;
        return services;
    }

    public static void UseDorbitLetsEncrypt(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel(options =>
        {
            var appServices = options.ApplicationServices;
            options.ConfigureHttpsDefaults(h =>
            {
                h.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                h.UseLettuceEncrypt(appServices);
            });
            // options.Listen(IPAddress.Any, 80);
            // options.Listen(IPAddress.Any, 443, o => o.UseHttps(h => { h.UseLettuceEncrypt(appServices); }));
        });
    }
}