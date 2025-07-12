using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dorbit.Framework.Extensions;

public static class AppDomainExtensions
{
    public static List<Assembly> GetAssemblies(this AppDomain appDomain, string[] prefixes)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        return allAssemblies.Where(x => prefixes.Any(prefix => x.FullName.StartsWith(prefix))).ToList();
    }
}