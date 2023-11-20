using Dorbit.Utils.Queries;

namespace Dorbit.Services.Abstractions;

public interface IQueryOptionsService
{
    IQueryOptionsService AddOptions<T>(QueryOptions options);
    IQueryOptionsService AddOptions(Type type, QueryOptions options);
    IQueryable<T> ApplyCountTo<T>(IQueryable<T> query);
    IQueryable<T> ApplyTo<T>(IQueryable<T> query);
}