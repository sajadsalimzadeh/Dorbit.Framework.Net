using Devor.Framework.Database.Abstractions;
using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Repositories.Abstractions;

namespace Devor.Framework.Repositories
{
    public class BaseRepository<T> : BaseWriterRepository<T>, IBaseRepository<T> where T : class, IEntity
    {
        public BaseRepository(IDbContext dbContext) : base(dbContext)
        {

        }
    }
}
