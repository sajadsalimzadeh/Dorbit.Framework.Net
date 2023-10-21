using AutoMapper;
using Dorbit.Entities;

namespace Dorbit.Extensions
{
    public static class MapperExtensions
    {
        public static T MapTo<T>(this Entity obj, IMapper mapper)
        {
            return mapper.Map<T>(obj);
        }
        public static List<T> MapTo<T>(this IEnumerable<Entity> obj, IMapper mapper)
        {
            return mapper.Map<List<T>>(obj);
        }
    }
}
