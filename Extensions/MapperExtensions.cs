using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Dorbit.Framework.Extensions;

public static class MapperExtensions
{
    public static T MapTo<T>(this object obj)
    {
        return App.Mapper.Map<T>(obj);
    }

    public static List<TResult> MapTo<T, TResult>(this List<T> obj)
    {
        return App.Mapper.Map<List<TResult>>(obj);
    }

    public static T MapTo<T>(this object obj, T model)
    {
        return App.Mapper.Map(obj, model);
    }

    public static async Task<TResult> MapToAsync<T, TResult>(this Task<T> task)
    {
        return App.Mapper.Map<TResult>(await task);
    }

    public static async Task<List<TResult>> MapToAsync<T, TResult>(this Task<List<T>> task)
    {
        return App.Mapper.Map<List<TResult>>(await task);
    }

    public static T PatchObject<T, TPatch>(this T model, object patch)
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
        var patchObject = JsonSerializer.Deserialize<TPatch>(jsonElement.GetRawText(), options);
        var resultProperties = typeof(T).GetProperties();
        var patchProperties = typeof(TPatch).GetProperties();
        foreach (var jsonProperty in jsonElement.EnumerateObject())
        {
            var resultProperty = resultProperties.FirstOrDefault(x => string.Equals(x.Name, jsonProperty.Name, StringComparison.CurrentCultureIgnoreCase));
            var patchProperty = patchProperties.FirstOrDefault(x => string.Equals(x.Name, jsonProperty.Name, StringComparison.CurrentCultureIgnoreCase));
            if (resultProperty is null || patchProperty is null) continue;
            var pathValue = patchProperty.GetValue(patchObject);
            resultProperty.SetValue(model, pathValue);
        }

        return model;
    }
}