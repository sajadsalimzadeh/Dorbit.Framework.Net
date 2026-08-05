using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dorbit.Framework.Extensions;

public static class JsonExtensions
{
    public static bool TryGetGuid(this JsonElement element, string propertyName, out Guid guid)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.TryGetGuid(out var id))
        {
            guid = id;
            return true;
        }

        guid = Guid.Empty;
        return false;
    }


    public static object ToObject(this JsonValue value)
    {
        var element = value.GetValue<JsonElement>();

        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();

            case JsonValueKind.String:
            {
                if (element.TryGetGuid(out var guid))
                    return guid;

                if (element.TryGetDateTime(out var dateTime))
                    return dateTime;

                if (element.TryGetDateTimeOffset(out var dto))
                    return dto;

                return element.GetString();
            }

            case JsonValueKind.Number:
            {
                if (element.TryGetByte(out var b))
                    return b;

                if (element.TryGetSByte(out var sb))
                    return sb;

                if (element.TryGetInt16(out var s))
                    return s;

                if (element.TryGetUInt16(out var us))
                    return us;

                if (element.TryGetInt32(out var i))
                    return i;

                if (element.TryGetUInt32(out var ui))
                    return ui;

                if (element.TryGetInt64(out var l))
                    return l;

                if (element.TryGetUInt64(out var ul))
                    return ul;

                if (element.TryGetDecimal(out var dec))
                    return dec;

                if (element.TryGetDouble(out var d))
                    return d;

                if (element.TryGetSingle(out var f))
                    return f;

                return element.GetRawText();
            }

            case JsonValueKind.Array:
                return value.AsArray();

            case JsonValueKind.Object:
                return value.AsObject();

            default:
                return element.GetRawText();
        }
    }
}