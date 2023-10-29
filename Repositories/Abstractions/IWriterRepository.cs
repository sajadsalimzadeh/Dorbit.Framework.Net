using Dorbit.Entities.Abstractions;

namespace Dorbit.Repositories.Abstractions
{
    public interface IWriterRepository<T> : IReaderRepository<T> where T : class, IEntity
    {
        T Insert(T model);
        T Insert<TR>(TR dto);
        T Update(T model);
        T Update<TR>(Guid id, TR dto);
        T Remove(Guid id);
        T Remove(T model);
    }
}
