using Dorbit.Database.Abstractions;
using Dorbit.Entities.Abstractions;
using Dorbit.Repositories.Abstractions;

namespace Dorbit.Repositories;

public class BaseRepository<T> : BaseWriterRepository<T>, IBaseRepository<T> where T : class, IEntity
{
    public BaseRepository(IDbContext dbContext) : base(dbContext)
    {

    }
}