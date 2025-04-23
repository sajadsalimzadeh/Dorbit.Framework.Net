using System;
using System.Collections.Generic;
using System.Reflection;
using Dorbit.Framework.Extensions;

namespace Dorbit.Framework.Utils.Collections;

public static class ListExtension
{
    public static bool Replace<T>(this List<T> list, Func<T, bool> func, T replacement)
    {
        var view = list.FindIndex(t => func(t));
        if (view > -1)
        {
            var type = replacement.GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.GetSetMethod() != null && prop.GetGetMethod() != null)
                    prop.SetValue(list[view], prop.GetValue(replacement));
            }

            return true;
        }

        return false;
    }

    public static List<T> SearchPrimaryTypes<T>(this List<T> list, T obj, int take = 0, bool enums = false)
    {
        return list.Search(obj, (prop, value) =>
        {
            if (!enums && value is Enum) return false;
            return value.GetType().IsNumeric() || value.GetType().IsString();
        });
    }

    public static List<T> Search<T>(this List<T> list, T obj, Func<PropertyInfo, object, bool> isValidPropery, int take = 0)
    {
        var props = new List<PropertyInfo>();
        var values = new Dictionary<string, string>();
        var test = Activator.CreateInstance<T>();
        foreach (var prop in obj.GetType().GetProperties())
        {
            var value1 = prop.GetValue(obj);
            var value2 = prop.GetValue(test);
            if (value1 == null || !isValidPropery(prop, value1) || value1.Equals(value2)) continue;
            props.Add(prop);
            values[prop.Name] = value1.ToString().ToLower();
        }

        if (props.Count == 0) return list;
        var result = new List<T>();
        foreach (var item in list)
        {
            if (take > 0 && result.Count == take) break;
            var flag = true;
            foreach (var prop in props)
            {
                var value1 = prop.GetValue(item).ToString().ToLower();
                var value2 = values[prop.Name];
                if (!value1.Contains(value2))
                {
                    flag = false;
                    break;
                }
            }

            if (flag) result.Add(item);
        }

        return result;
    }
}