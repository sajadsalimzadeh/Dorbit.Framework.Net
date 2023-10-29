using AutoMapper;
using Dorbit.Database.Abstractions;
using Dorbit.Entities.Abstractions;
using Dorbit.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Repositories
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

        public T Remove(Guid id)
        {
            return Remove(GetById(id));
        }

        public T Update<TR>(Guid id, TR dto)
        {
            var mapper = dbContext.ServiceProvider.GetService<IMapper>();
            return Update(mapper.Map(dto, GetById(id)));
        }
    }
}
