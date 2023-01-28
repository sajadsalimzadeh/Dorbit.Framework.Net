using Devor.Framework.Database.Abstractions;
using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Models;
using Devor.Framework.Utils.Queries;
using Devor.Framework.Repositories.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace Devor.Framework.Repositories
{
    public class BaseReaderRepository<T> : IReaderRepository<T> where T : class, IEntity
    {
        private readonly IDbContext dbContext;

        protected IServiceProvider ServiceProvider => dbContext.ServiceProvider;

        public BaseReaderRepository(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public virtual IQueryable<T> Set(bool excludeDeleted = true)
        {
            return dbContext.DbSet<T>(excludeDeleted);
        }

        public virtual int Count()
        {
            return Set().Count();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Set().ToList();
        }

        public IEnumerable<T> GetAllWithCache(TimeSpan? timeSpan = null)
        {
            var memoryCache = ServiceProvider.GetService<IMemoryCache>();
            var key = $"{GetType().Name}-{typeof(T).Name}-{nameof(GetAllWithCache)}";
            if (!memoryCache.TryGetValue<IEnumerable<T>>(key, out var items))
            {
                items = GetAll();
                memoryCache.Set(key, items, timeSpan ?? TimeSpan.FromMinutes(1));
            }
            return items;
        }

        public virtual T GetById(long id)
        {
            return Set().FirstOrDefault(x => x.Id == id);
        }

        public T GetByIdWithCache(long id, TimeSpan? timeSpan = null)
        {
            var memoryCache = ServiceProvider.GetService<IMemoryCache>();
            var key = $"{GetType().Name}-{typeof(T).Name}-{nameof(GetByIdWithCache)}";
            if (!memoryCache.TryGetValue<T>(key, out var item))
            {
                item = GetById(id);
                memoryCache.Set(key, item, timeSpan ?? TimeSpan.FromMinutes(1));
            }
            return item;
        }

        public virtual T First()
        {
            return Set().FirstOrDefault();
        }

        public virtual T Last()
        {
            return Set().OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public virtual PagedListResult<T> Select(QueryOptions queryOptions)
        {
            return Set().ApplyToPagedList(queryOptions);
        }
    }
}
