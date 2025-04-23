using System;
using System.Text.Json;
using AutoMapper.Internal;

namespace Dorbit.Framework.Extensions;

public static class ConvertExtensions
{
    public static object ConvertTo(this object value, Type type)
    {
        if (type.IsNullableType())
        {
            type = Nullable.GetUnderlyingType(type);
        }

        var valueType = value.GetType();
        if (valueType == type) return value;
        if (valueType.IsString())
        {
            if (value.ToString().IsNullOrEmpty()) return null;
            if (type == typeof(Guid)) return Guid.Parse(value.ToString() ?? string.Empty);
            if (type == typeof(DateTime)) return DateTime.Parse(value.ToString() ?? string.Empty);
        }
        else if (type.IsNumeric())
        {
            if (type == typeof(long)) return long.Parse(value.ToString() ?? string.Empty);
            if (type == typeof(int)) return int.Parse(value.ToString() ?? string.Empty);
            if (type == typeof(short)) return short.Parse(value.ToString() ?? string.Empty);
            if (type == typeof(byte)) return byte.Parse(value.ToString() ?? string.Empty);
        }
        else if (type.IsEnum)
        {
            return Enum.Parse(type, value.ToString() ?? string.Empty);
        }

        return JsonSerializer.Deserialize(JsonSerializer.Serialize(value), type);
    }
}