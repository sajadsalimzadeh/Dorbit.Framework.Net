using System;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Extensions;

public static class OptionsExtensions
{
    public static T ValueWithNullCheck<T>(this IOptions<T> options) where T : class
    {
        var value = options.Value;
        if(value == null)
            throw new Exception($"{typeof(T).Name} not configured");
        return value;
    }
}