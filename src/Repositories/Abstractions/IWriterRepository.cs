using Devor.Framework.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devor.Framework.Repositories.Abstractions
{
    public interface IWriterRepository<T> : IReaderRepository<T> where T : class, IEntity
    {
        T Insert(T model);
        T Insert<TR>(TR dto);
        T Update(T model);
        T Update<TR>(long id, TR dto);
        T Remove(long id);
        T Remove(T model);
    }
}
