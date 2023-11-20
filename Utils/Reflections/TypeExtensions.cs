using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Dorbit.Utils.Reflections;

public static class TypeExtensions
{
    public static PropertyInfo GetKey(this Type type) => type.GetPropertyByAttribute<KeyAttribute>();
    public static PropertyInfo GetPropertyByAttribute<T>(this Type type) where T : Attribute
    {
        var attrType = typeof(T);
        foreach (var property in type.GetProperties()) if (Attribute.GetCustomAttribute(property, attrType) != null) return property;
        return null;
    }
    public static List<PropertyInfo> GetPropertiesByAttribute<T>(this Type type) where T : Attribute
    {
        var attrType = typeof(T);
        var list = new List<PropertyInfo>();
        foreach (var property in type.GetProperties()) if (Attribute.GetCustomAttribute(property, attrType) != null) list.Add(property);
        return list;
    }
}