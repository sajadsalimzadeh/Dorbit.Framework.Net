using System;
using System.Text.Json;

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
}