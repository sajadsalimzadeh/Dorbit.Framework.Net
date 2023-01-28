using Devor.Framework.Utils.Queries;
using System;
using System.Linq;

namespace Devor.Framework.Services.Abstractions
{
    public interface IQueryOptionsService
    {
        IQueryOptionsService AddOptions<T>(QueryOptions options);
        IQueryOptionsService AddOptions(Type type, QueryOptions options);
        IQueryable<T> ApplyCountTo<T>(IQueryable<T> query);
        IQueryable<T> ApplyTo<T>(IQueryable<T> query);
    }
}
