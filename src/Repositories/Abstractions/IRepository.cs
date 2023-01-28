using Devor.Framework.Entities.Abstractions;
using System;

namespace Devor.Framework.Repositories.Abstractions
{
    public interface IBaseRepository<T> : IWriterRepository<T> where T : class, IEntity
    {
    }
}
