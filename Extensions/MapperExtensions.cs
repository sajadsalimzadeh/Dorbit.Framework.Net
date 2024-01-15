using System.Threading.Tasks;

namespace Dorbit.Framework.Extensions;

public static class MapperExtensions
{

    public static T MapTo<T>(this object obj)
    {
        return App.Mapper.Map<T>(obj);
    }
    
    public static T MapTo<T>(this object obj, T model)
    {
        return App.Mapper.Map(obj, model);
    }
    
    public static async Task<TResult> MapAsync<TSource, TResult>(this Task<TSource> task)
    {
        return App.Mapper.Map<TResult>(await task);
    }
}