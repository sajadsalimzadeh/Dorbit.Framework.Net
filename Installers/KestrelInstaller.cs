using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Dorbit.Framework.Installers;

public static class KestrelInstaller
{
    public static WebApplicationBuilder SecureKestrel(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel(config => { config.AddServerHeader = false; });
        return builder;
    }

    public static ConfigureWebHostBuilder UseKestrel(this ConfigureWebHostBuilder builder, Configs configs)
    {
        builder.UseKestrel(options =>
        {
            options.ListenAnyIP(configs.Port);
            if (configs.Https is not null)
            {
                options.ListenAnyIP(configs.Https.Port, x => x.UseHttps(configs.Https.Filename, configs.Https.Password));
            }
        });

        var urls = new List<string>() { $"http://*:{configs.Port}" };

        if (configs.Https is not null)
        {
            urls.Add($"https://*:{configs.Https.Port}");
        }

        builder.UseUrls(urls.ToArray());
        return builder;
    }

    public class Configs
    {
        public required int Port { get; init; }

        public ConfigsHttps Https { get; init; }
    }

    public class ConfigsHttps
    {
        public int Port { get; init; }
        public string Filename { get; init; }
        public string Password { get; init; }
    }
}