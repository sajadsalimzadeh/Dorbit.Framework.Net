using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public static T Patch<T>(this T model, object patch)
    {
        var modelProperties = model.GetType().GetProperties();
        var patchProperties = patch.GetType().GetProperties();
        foreach (var patchProperty in patchProperties)
        {
            var modelProperty = modelProperties.FirstOrDefault(x => x.Name == patchProperty.Name);
            if (modelProperty is null) continue;
            
            var value = patchProperty.GetValue(patch);
            if (value == default) continue;
            
            modelProperty.SetValue(model, value);
        }

        return model;
    }
}