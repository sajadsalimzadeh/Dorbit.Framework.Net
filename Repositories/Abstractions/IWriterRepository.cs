using System;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IWriterRepository<T> : IReaderRepository<T> where T : class, IEntity
{
    Task<T> InsertAsync(T model);
    Task<T> InsertAsync<TR>(TR dto);
    Task<T> UpdateAsync(T model);
    Task<T> UpdateAsync<TR>(Guid id, TR dto);
    Task<T> RemoveAsync(Guid id);
    Task<T> RemoveAsync(T model);
}