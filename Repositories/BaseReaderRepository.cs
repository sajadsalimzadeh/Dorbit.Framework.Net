using Dorbit.Database.Abstractions;
using Dorbit.Entities.Abstractions;
using Dorbit.Models;
using Dorbit.Repositories.Abstractions;
using Dorbit.Utils.Queries;

namespace Dorbit.Repositories
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

        public virtual T GetById(Guid id)
        {
            return Set().FirstOrDefault(x => x.Id == id);
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
