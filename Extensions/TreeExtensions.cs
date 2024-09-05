using System;
using System.Collections.Generic;
using Dorbit.Framework.Contracts.Trees;

namespace Dorbit.Framework.Extensions;

public static class TreeExtensions
{
    public static void ForEachRecursively<T>(this List<T> items, Action<T> before = null, Action<T> after = null) where T : ITree<T>
    {
        foreach (var item in items)
        {
            before?.Invoke(item);
            item.Children?.ForEachRecursively(before, after);
            after?.Invoke(item);
        }
    }

    public static T Find<T>(this List<T> items, Predicate<T> predicate) where T : ITree<T>
    {
        foreach (var item in items)
        {
            if (predicate(item)) return item;
            if (item.Children is null) continue;
            var result = item.Children.Find(predicate);
            if (result is not null) return result;
        }

        return default;
    }
}