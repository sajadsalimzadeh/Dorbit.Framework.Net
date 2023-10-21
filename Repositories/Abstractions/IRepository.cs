using Dorbit.Entities.Abstractions;

namespace Dorbit.Repositories.Abstractions
{
    public interface IBaseRepository<T> : IWriterRepository<T> where T : class, IEntity
    {
    }
}
