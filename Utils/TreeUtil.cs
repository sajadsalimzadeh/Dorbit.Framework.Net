using System;
using System.Collections.Generic;
using System.Linq;

namespace Dorbit.Framework.Utils;

public static class TreeUtil
{
    public static List<T> Flatten<T>(IEnumerable<T> items, Func<T, IEnumerable<T>> getChildren, Action<T, T> setParent = null)
    {
        var result = new List<T>();
        foreach (var item in items)
        {
            result.Add(item);
            var children = getChildren(item)?.ToList();
            if (children is not null && children.Count != 0)
            {
                result.AddRange(Flatten(children, getChildren));
                if (setParent != null)
                {
                    foreach (var child in children)
                    {
                        setParent(child, item);
                    }
                }
            }
        }
        return result;
    }
}