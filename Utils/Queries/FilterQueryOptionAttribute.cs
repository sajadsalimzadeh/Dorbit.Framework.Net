using System;
using System.Linq;

namespace Dorbit.Framework.Utils.Queries;

[AttributeUsage(AttributeTargets.All)]
internal class FilterQueryOptionSqlAttribute(string value) : Attribute
{
    public string Value { get; set; } = value;
}

[AttributeUsage(AttributeTargets.All)]
internal class FilterQueryOptionFormatAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
}

internal static class FilterQueryOptionAttributeExtension
{
    public static string GetSqlValue(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        return type.GetField(name).GetCustomAttributes(false).OfType<FilterQueryOptionSqlAttribute>().SingleOrDefault()?.Value;
    }

    public static string GetFormat(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        return type.GetField(name).GetCustomAttributes(false).OfType<FilterQueryOptionFormatAttribute>().SingleOrDefault()?.Format;
    }
}