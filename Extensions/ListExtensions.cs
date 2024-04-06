using System;
using System.Collections.Generic;
using System.Linq;

namespace Dorbit.Framework.Extensions;

public static class ListExtensions
{
    private static Random _rng = new();

    public static List<T> Shuffle<T>(this List<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> items)
    {
        return items.Select((value, index) => new { Index = index, Value = value }).ToDictionary(x => x.Index, x => x.Value);
    }

    public static T GetOrAdd<T>(this List<T> list, Predicate<T> predicate, Func<T> addFunction)
    {
        var item = list.Find(predicate);
        if (item is null)
        {
            item = addFunction();
            list.Add(item);
        }

        return item;
    }
}