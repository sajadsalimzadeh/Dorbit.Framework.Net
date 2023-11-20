namespace Dorbit.Utils.Queries;

[AttributeUsage(AttributeTargets.All)]
internal class FilterQueryOptionSqlAttribute : Attribute
{
    public string Value { get; set; }
    public FilterQueryOptionSqlAttribute(string value)
    {
        Value = value;
    }
}
[AttributeUsage(AttributeTargets.All)]
internal class FilterQueryOptionFormatAttribute : Attribute
{
    public string Format { get; set; }

    public FilterQueryOptionFormatAttribute(string format)
    {
        Format = format;
    }
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