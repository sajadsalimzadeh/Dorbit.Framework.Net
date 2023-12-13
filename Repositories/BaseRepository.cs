using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;

namespace Dorbit.Framework.Repositories;

public class BaseRepository<T> : BaseWriterRepository<T>, IBaseRepository<T> where T : class, IEntity
{
    public BaseRepository(IDbContext dbContext) : base(dbContext)
    {

    }
}