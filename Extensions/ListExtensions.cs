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
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }

    public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> items)
    {
        return items.Select((value, index) => new { Index = index, Value = value }).ToDictionary(x => x.Index, x => x.Value);
    }

    public static List<List<T>> Chunk<T>(this List<T> items, int size)
    {
        var result = new List<List<T>>();

        for (var i = 0; i < items.Count; i += size)
        {
            result.Add(items.Skip(i).Take(size).ToList());
        }

        return result;
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

    public static double AverageOrDefault<T>(this IEnumerable<T> items, Func<T, double> func)
    {
        var enumerable = items as T[] ?? items.ToArray();
        return enumerable.Length != 0 ? enumerable.Average(func) : 0;
    }

    public static bool IsEqualBy<T>(this IEnumerable<T> list1, IEnumerable<T> list2, Func<T, T, bool> predict)
    {
        if (list2 is null && list1 is null) return true;
        if (list1 is null) return false;
        if (list2 is null) return false;

        var items = (list2 as List<T> ?? list2.ToList());
        foreach (var item in list1)
        {
            if (items.Any(x => predict(x, item))) return false;
        }

        return true;
    }
}