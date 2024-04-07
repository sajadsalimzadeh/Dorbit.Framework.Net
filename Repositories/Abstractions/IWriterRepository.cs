using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IWriterRepository<T> : IReaderRepository<T> where T : class, IEntity
{
    Task<T> InsertAsync(T model);
    Task BulkInsertAsync(List<T> enitites);
    Task<T> UpdateAsync(T model);
    Task BulkUpdateAsync(List<T> enitites);
    Task<T> DeleteAsync(T model);
    Task BulkDeleteAsync(List<T> enitites);

    Task<T> InsertAsync<TR>(TR dto);
    Task<T> UpdateAsync<TR>(Guid id, TR dto);
    Task<T> DeleteAsync(Guid id);
}