using AutoMapper;
using Devor.Framework.Database.Abstractions;
using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Devor.Framework.Repositories
{
    public class BaseWriterRepository<T> : BaseReaderRepository<T>, IWriterRepository<T> where T : class, IEntity
    {
        private readonly IDbContext dbContext;

        public BaseWriterRepository(IDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public virtual T Insert(T model)
        {
            return dbContext.InsertEntity(model);
        }

        public virtual T Remove(T model)
        {
            return dbContext.RemoveEntity(model);
        }

        public virtual T Update(T model)
        {
            return dbContext.UpdateEntity(model);
        }
        //================== Extended Methods ==================\\
        public T Insert<TR>(TR dto)
        {
            var mapper = dbContext.ServiceProvider.GetService<IMapper>();
            return Insert(mapper.Map<T>(dto));
        }

        public T Remove(long id)
        {
            return Remove(GetById(id));
        }

        public T Update<TR>(long id, TR dto)
        {
            var mapper = dbContext.ServiceProvider.GetService<IMapper>();
            return Update(mapper.Map(dto, GetById(id)));
        }
    }
}
