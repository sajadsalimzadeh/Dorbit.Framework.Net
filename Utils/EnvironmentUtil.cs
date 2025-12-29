using System;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace Dorbit.Framework.Utils;

public static class EnvironmentUtil
{
    public static string GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
#if DEBUG
        if (string.IsNullOrEmpty(environment)) environment = Environments.Development;
#else
        if(string.IsNullOrEmpty(environment)) environment = Environments.Production;
#endif
        return environment?.ToLower() ?? "development";
    }

    public static bool IsDevelopment()
    {
        if (InMemory()) return true;
        return string.Equals(GetEnvironment(), Environments.Development, StringComparison.CurrentCultureIgnoreCase);
    }
    
    public static bool IsProduction()
    {
        return !IsDevelopment();
    }

    public static string GetArgValue(string name)
    {
        name = $"--{name}=";
        var args = Environment.GetCommandLineArgs();
        return args.FirstOrDefault(x => x.StartsWith(name))?.Replace(name, "");
    }

    public static bool InMemory()
    {
        var args = Environment.GetCommandLineArgs();
        return args.Contains("inmemory");
    }
}