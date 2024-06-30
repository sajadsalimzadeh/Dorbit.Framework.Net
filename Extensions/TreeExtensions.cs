using System;
using System.Collections.Generic;
using Dorbit.Framework.Contracts.Trees;

namespace Dorbit.Framework.Extensions;

public static class TreeExtensions
{
    public static void ForEachRecursively<T>(this T item, Action<T> before = null, Action<T> after = null) where T : ITree<T>
    {
        before?.Invoke(item);
        if (item.Children is not null)
        {
            foreach (var child in item.Children)
            {
                child.ForEachRecursively(before, after);
            }
        }

        after?.Invoke(item);
    }
}