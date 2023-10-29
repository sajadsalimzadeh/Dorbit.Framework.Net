using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Extensions;

public static class AutoMapperExtensions
{
    private static object monitor = new {};
    private static IMapper _mapper;
        private static IMapper Mapper
    {
        get
        {
            if (_mapper is not null) return _mapper;
            lock (monitor)
            {
                if (_mapper is null)
                {
                    _mapper = App.ServiceProvider.GetService<IMapper>();
                }
            }

            return _mapper;
        }
    }
        
    public static T MapTo<T>(this object obj)
    {
        return Mapper.Map<T>(obj);
    }
        
    public static T MapTo<T>(this object obj, T dest)
    {
        return Mapper.Map(obj, dest);
    }
    
    public static T MapTo<T>(this object obj, IMapper mapper)
    {
        return mapper.Map<T>(obj);
    }
    public static List<T> MapTo<T>(this IEnumerable<object> items, IMapper mapper)
    {
        return mapper.Map<List<T>>(items);
    }
}