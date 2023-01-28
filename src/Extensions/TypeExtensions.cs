using System;

namespace Devor.Framework.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(byte)) return true;
            if (type == typeof(sbyte)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(ushort)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(uint)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(ulong)) return true;
            if (type == typeof(float)) return true;
            if (type == typeof(double)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(char)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(DateTime)) return true;
            if (type == typeof(TimeSpan)) return true;
            if (type == typeof(byte?)) return true;
            if (type == typeof(sbyte?)) return true;
            if (type == typeof(short?)) return true;
            if (type == typeof(ushort?)) return true;
            if (type == typeof(int?)) return true;
            if (type == typeof(uint?)) return true;
            if (type == typeof(long?)) return true;
            if (type == typeof(ulong?)) return true;
            if (type == typeof(float?)) return true;
            if (type == typeof(double?)) return true;
            if (type == typeof(decimal?)) return true;
            if (type == typeof(char?)) return true;
            if (type == typeof(bool?)) return true;
            if (type == typeof(DateTime?)) return true;
            if (type == typeof(TimeSpan?)) return true;
            if (typeof(Enum).IsAssignableFrom(type)) return true;
            return false;
        }
    }
}
