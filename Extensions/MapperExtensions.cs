using System.Collections.Generic;
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
}