using AutoMapper;
using Dorbit.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Extensions;

public static class MapperExtensions
{
    private static IMapper _mapper;
    private static IMapper Mapper => _mapper ??= FrameworkInstaller.ServiceProvider.GetService<IMapper>();

    public static T MapTo<T>(this object obj)
    {
        return Mapper.Map<T>(obj);
    }
    
    public static T MapTo<T>(this object obj, T model)
    {
        return Mapper.Map(obj, model);
    }
    
    public static async Task<T> MapToAsync<T>(this Task<object> task)
    {
        return Mapper.Map<T>(await task);
    }
}