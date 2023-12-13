using System.Reflection;

namespace Dorbit.Framework.Extensions;

public static class AssemblyExtensions
{
    public static List<Assembly> GetAssemblies(this Assembly entryAssembly, string[] namespaces)
    {
        var returnAssemblies = new List<Assembly>();
        var loadedAssemblies = new List<string>();
        var assembliesToCheck = new Queue<Assembly>();
        assembliesToCheck.Enqueue(entryAssembly);
        while (assembliesToCheck.Any())
        {
            var assemblyToCheck = assembliesToCheck.Dequeue();
            foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
            {
                if (loadedAssemblies.Contains(reference.FullName)) continue;
                try
                {
                    var assembly = Assembly.Load(reference);
                    assembliesToCheck.Enqueue(assembly);
                }
                catch
                {
                    // ignored
                }

                loadedAssemblies.Add(reference.FullName);
            }

            if (namespaces.Any(x => assemblyToCheck.FullName != null && assemblyToCheck.FullName.StartsWith(x)))
            {
                returnAssemblies.Add(assemblyToCheck);
            }
        }

        return returnAssemblies;
    }
}