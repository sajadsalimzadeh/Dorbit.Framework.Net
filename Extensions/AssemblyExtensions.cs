using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dorbit.Framework.Extensions;

public static class AssemblyExtensions
{
    public static List<Assembly> GetAllAssembly(this Assembly root, string[] prefixes)
    {
        var visited = new List<Assembly>();
        var queue = new Queue<Assembly>();

        queue.Enqueue(root);

        while (queue.Any())
        {
            var assembly = queue.Dequeue();
            var assemblyName = assembly.GetName().FullName ?? "";
            if (visited.Any(x => x.FullName == assemblyName)) continue;
            visited.Add(assembly);

            var references = assembly.GetReferencedAssemblies();
            foreach (var reference in references)
            {
                if (visited.Any(x => x.FullName == reference.FullName)) continue;
                if (prefixes.Any(x => reference.Name.StartsWith(x)))
                {
                    queue.Enqueue(Assembly.Load(reference));
                }
            }
        }

        return visited;
    }
}