using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

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

    public static string FirstValueOrDefault(this IEnumerable<KeyValuePair<string, StringValues>> items, string key)
    {
        return items.FirstOrDefault(x => x.Key == key).Value.ToString() ?? string.Empty;
    }

    public static string FirstValueOrDefault(this IEnumerable<KeyValuePair<string, string>> items, string key)
    {
        return items.FirstOrDefault(x => x.Key == key).Value.ToString();
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
    {
        return items is null || !items.Any();
    }

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> items)
    {
        return items is not null && items.Any();
    }

    public static bool AddIfNotNull<T>(this List<T> items, T obj)
    {
        if (obj == null) return false;
        items.Add(obj);
        return true;
    }
}