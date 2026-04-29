using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Mappers;
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

    public static T PatchObject<T, TPatch>(this T model, TPatch patch, MapperPatchOptions options = null)
    {
        if (patch is JsonElement jsonElement) return model.PatchObjectWithJson<T, T>(jsonElement, options);
        
        var json = JsonSerializer.Serialize(patch);
        var doc = JsonDocument.Parse(json);
        jsonElement = doc.RootElement;
        return model.PatchObjectWithJson<T, TPatch>(jsonElement, options);
    }

    public static T PatchObjectWithJson<T, TPatch>(this T model, JsonElement jsonElement, MapperPatchOptions options = null)
    {
        options ??= new MapperPatchOptions();
        var serializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNameCaseInsensitive = true
        };
        var patchObject = JsonSerializer.Deserialize<TPatch>(jsonElement.GetRawText(), serializerOptions);
        var resultProperties = typeof(T).GetProperties();
        var patchProperties = typeof(TPatch).GetProperties();
        foreach (var jsonProperty in jsonElement.EnumerateObject())
        {
            var resultProperty = resultProperties.FirstOrDefault(x => string.Equals(x.Name, jsonProperty.Name, StringComparison.CurrentCultureIgnoreCase));
            var patchProperty = patchProperties.FirstOrDefault(x => string.Equals(x.Name, jsonProperty.Name, StringComparison.CurrentCultureIgnoreCase));
            if (resultProperty is null || patchProperty is null) continue;
            if(options.IgnoreProperties.Contains(resultProperty.Name)) continue;
            var pathValue = patchProperty.GetValue(patchObject);
            resultProperty.SetValue(model, pathValue);
        }

        return model;
    }
}