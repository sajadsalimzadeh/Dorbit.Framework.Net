using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IBaseRepository<T> : IWriterRepository<T> where T : class, IEntity
{
}