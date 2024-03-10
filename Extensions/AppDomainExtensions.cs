using System;
using Microsoft.Extensions.Hosting;

namespace Dorbit.Framework.Extensions;

public static class AppDomainExtensions
{
    public static string GetEnvironment(this AppDomain domain)
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }

    public static bool IsDevelopment(this AppDomain domain)
    {
        return GetEnvironment(domain) == Environments.Development;
    }
}