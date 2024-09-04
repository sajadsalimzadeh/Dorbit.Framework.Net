using System;
using Dorbit.Framework.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Dorbit.Framework.Extensions;

public static class WebApplicationBuilderExtensions
{

    public static IConfiguration BuildDorbit(this ConfigurationManager configuration)
    {
        return BuildDorbit(configuration.AddJsonFile("appsettings.json", true));
    }

    public static IConfiguration BuildDorbit(this IConfigurationBuilder configurationBuilder)
    {
        var environment = EnvironmentUtil.GetEnvironment();
        configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
        configurationBuilder.AddJsonFile("appsettings.json", true);
        configurationBuilder.AddJsonFile($"appsettings.{environment}.json", true);
        configurationBuilder.AddJsonFile("appsettings.custom.json", true);
        return configurationBuilder.Build();
    }
}