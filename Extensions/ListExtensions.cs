using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Extensions;

public static class ListExtensions
{
    private static Random _rng = new();
    
    public static List<T> Shuffle<T>(this List<T> list)  
    {  
        var n = list.Count;  
        while (n > 1) {  
            n--;  
            var k = _rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }
        return list;
    }
}