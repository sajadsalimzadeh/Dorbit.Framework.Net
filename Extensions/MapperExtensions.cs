using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Dorbit.Framework.Extensions;

public static class MapperExtensions
{
    public static T MapTo<T>(this object obj)
    {
        return App.Mapper.Map<T>(obj);
    }

    public static List<TR> MapTo<T, TR>(this List<T> obj)
    {
        return App.Mapper.Map<List<TR>>(obj);
    }

    public static T MapTo<T>(this object obj, T model)
    {
        return App.Mapper.Map(obj, model);
    }

    public static async Task<TR> MapToAsync<T, TR>(this Task<T> task)
    {
        return App.Mapper.Map<TR>(await task);
    }

    public static async Task<List<TR>> MapToAsync<T, TR>(this Task<List<T>> task)
    {
        return App.Mapper.Map<List<TR>>(await task);
    }

    public static T PatchObject<T>(this T model, object patch)
    {
        if (patch is not JsonElement jsonElement)
        {
            var json = JsonSerializer.Serialize(patch);
            var doc = JsonDocument.Parse(json);
            jsonElement = doc.RootElement;
        }
        
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNameCaseInsensitive = true
        };
        var patchObject = JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), options);
        var properties = typeof(T).GetProperties();
        var jsonProperties = jsonElement.EnumerateObject();
        foreach (var jsonProperty in jsonProperties)
        {
            var property = properties.FirstOrDefault(x => string.Equals(x.Name, jsonProperty.Name, StringComparison.CurrentCultureIgnoreCase));
            if (property is null) continue;
            var pathValue = property.GetValue(patchObject);
            property.SetValue(model, pathValue);
        }

        return model;
    }
}