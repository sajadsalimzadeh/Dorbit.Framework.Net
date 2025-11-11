using System;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dorbit.Framework.Extensions;

public static class SettingExtensions
{
    public static TResult GetValueOrDefault<T, TResult>(this T setting, Func<T, TResult> func)
    {
        var result = func.Invoke(setting);
        if (result == null || typeof(TResult).GetDefaultValue().Equals(result))
        {
            var instance = Activator.CreateInstance<T>();
            return func(instance);
        }

        return result;
    }
}