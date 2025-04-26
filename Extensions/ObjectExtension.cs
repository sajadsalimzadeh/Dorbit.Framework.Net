using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dorbit.Framework.Extensions;

public static class ObjectExtension
{
    public static T Clone<T>(this T obj)
    {
        var type = obj.GetType();
        var tmp = Activator.CreateInstance<T>();
        foreach (var item in type.GetProperties())
        {
            item.SetValue(tmp, item.GetValue(obj));
        }

        return tmp;
    }

    public static T Overwrite<T>(this T obj1, T obj2, bool ignoreDefaults = false)
    {
        if (ignoreDefaults)
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var property in obj1.GetType().GetProperties())
            {
                var value = property.GetValue(obj2);
                if (value.Equals(property.GetValue(obj))) continue;
                property.SetValue(obj1, value);
            }
        }
        else
        {
            foreach (var property in obj1.GetType().GetProperties()) property.SetValue(obj1, property.GetValue(obj2));
        }

        return obj1;
    }

    public static T Overwrite<T, TR>(this T obj1, TR obj2, bool ignoreDefaults = false) where T : TR
    {
        foreach (var property in obj2.GetType().GetProperties()) property.SetValue(obj1, property.GetValue(obj2));
        return obj1;
    }

    public static Dictionary<string, object> ToDictionary(this object o)
    {
        return o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(o, null));
    }

    public static T Patch<T>(this Dictionary<string, object> dict, T destination)
    {
        var destType = typeof(T);
        var destProperties = destType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var item in dict)
        {
            var destProperty = destProperties.FirstOrDefault(x => string.Equals(x.Name, item.Key, StringComparison.CurrentCultureIgnoreCase));
            if (destProperty is null) continue;
            if (item.Value is null)
            {
                destProperty.SetValue(destination, null);
                continue;
            }

            destProperty.SetValue(destination, item.Value.ConvertTo(destProperty.PropertyType));
        }

        return destination;
    }

    public static T Patch<T>(this object source, T destination)
    {
        var sourceType = source.GetType();
        var destType = typeof(T);
        var sourceProperties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var destProperties = destType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var sourceProperty in sourceProperties)
        {
            var destProperty = destProperties.FirstOrDefault(
                x => string.Equals(x.Name, sourceProperty.Name, StringComparison.CurrentCultureIgnoreCase) && x.PropertyType == sourceProperty.PropertyType);
            destProperty?.SetValue(destination, sourceProperty.GetValue(source));
        }

        return destination;
    }
}