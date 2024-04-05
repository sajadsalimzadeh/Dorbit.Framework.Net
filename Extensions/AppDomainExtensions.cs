using System;
using Microsoft.Extensions.Hosting;

namespace Dorbit.Framework.Extensions;

public static class AppDomainExtensions
{
    public static string GetEnvironment(this AppDomain domain)
    {
        #if !DEBUG
        return Environments.Production;
        #endif
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }

    public static bool IsDevelopment(this AppDomain domain)
    {
        return GetEnvironment(domain).ToLower() == Environments.Development.ToLower();
    }
}