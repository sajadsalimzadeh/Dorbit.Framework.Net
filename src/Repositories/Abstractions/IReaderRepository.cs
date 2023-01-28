using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Models;
using Devor.Framework.Utils.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devor.Framework.Repositories.Abstractions
{
    public interface IReaderRepository<T> where T : class, IEntity
    {
        T GetById(long id);
        T GetByIdWithCache(long id, TimeSpan? timeSpan = null);
        IQueryable<T> Set(bool excludeDeleted = true);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllWithCache(TimeSpan? timeSpan = null);
        PagedListResult<T> Select(QueryOptions queryOptions);
        T First();
        T Last();
        int Count();
    }
}
