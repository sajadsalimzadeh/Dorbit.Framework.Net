using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Database.Abstractions;

public interface IDbContext
{
    IServiceProvider ServiceProvider { get; }

    IDbContext AutoExcludeDeletedEnable();
    IDbContext AutoExcludeDeletedDisable();

    IQueryable<T> DbSet<T>() where T : class, IEntity;
    IQueryable<T> DbSet<T>(bool excludeDeleted) where T : class, IEntity;
    Task<T> InsertEntityAsync<T>(T model) where T : class, IEntity;
    Task<T> UpdateEntityAsync<T>(T model) where T : class, IEntity;
    Task<T> RemoveEntityAsync<T>(T model) where T : class, IEntity;
    Task<List<T>> QueryAsync<T>(string query, Dictionary<string, object> parameters);

    ITransaction BeginTransaction();
    int SaveChanges();
    IEnumerable<Type> GetLookupEntities();

    Task MigrateAsync();
}