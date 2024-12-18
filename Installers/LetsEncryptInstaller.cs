using System;
using System.IO;
using System.Net;
using LettuceEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dorbit.Framework.Installers;

public static class LetsEncryptInstaller
{
    public static void UseDorbitLetsEncrypt(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        if (builder.Environment.IsProduction() && configuration.GetSection("LettuceEncrypt").Exists())
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../Certificates/");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            builder.Services.AddLettuceEncrypt(options =>
                {
                    options.RenewDaysInAdvance = TimeSpan.FromDays(5);
                })
                .PersistDataToDirectory(new DirectoryInfo(path), "123456");

            builder.WebHost.UseKestrel(options =>
            {
                var appServices = options.ApplicationServices;
                options.Listen(IPAddress.Any, 80);
                options.Listen(
                    IPAddress.Any, 443,
                    o => o.UseHttps(h => { h.UseLettuceEncrypt(appServices); }));
            });
        }
    }
}