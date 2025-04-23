using System;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;

namespace Dorbit.Framework.Extensions;

public static class TypeExtensions
{
    public static T GetStaticPropertyValue<T>(this Type type, string name) where T : class
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(x => x.Name.Equals(name))?.GetValue(null) as T;
    }

    public static bool IsNumeric(this Type type)
    {
        if (type.IsNullableType())
        {
            type = Nullable.GetUnderlyingType(type);
        }
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    public static bool IsString(this Type type)
    {
        return type == typeof(string);
    }
}