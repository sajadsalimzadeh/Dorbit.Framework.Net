using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

    public static T PatchObject<T>(this T model, object patch, Type pathType = null)
    {
        if (patch is not JsonElement jsonElement)
        {
            var json = JsonSerializer.Serialize(patch);
            var doc = JsonDocument.Parse(json);
            jsonElement = doc.RootElement;
        }

        var patchObject = JsonConvert.DeserializeObject<T>(jsonElement.GetRawText());
        var patchProperties = (pathType ?? typeof(T)).GetProperties();
        var properties = typeof(T).GetProperties();
        foreach (var jsonProperty in jsonElement.EnumerateObject())
        {
            var property = properties.FirstOrDefault(x => string.Equals(x.Name, jsonProperty.Name, StringComparison.CurrentCultureIgnoreCase));
            if (property is null) continue;
            if (patchProperties.All(x => x.Name != property.Name)) continue;
            var pathValue = property.GetValue(patchObject);
            if(pathValue is null) continue;
            property.SetValue(model, pathValue);
        }

        return model;
    }
}