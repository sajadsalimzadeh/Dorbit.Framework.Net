using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Database.Abstractions;

public interface IDbContext
{
    IServiceProvider ServiceProvider { get; }
    bool AutoExcludeDeleted { get; set; }
    CancellationToken CancellationToken { get; set; }
    DatabaseProviderType ProviderType { get; }

    IQueryable<TEntity> DbSet<TEntity, TKey>(bool? excludeDeleted = null) where TEntity : class, IEntity<TKey>;
    IQueryable<TEntity> DbSet<TEntity>(bool? excludeDeleted = null) where TEntity : class, IEntity<Guid>;

    Task<TEntity> InsertEntityAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>;
    Task<TEntity> InsertEntityAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<Guid>;

    Task<TEntity> UpdateEntityAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>;
    Task<TEntity> UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<Guid>;

    Task<TEntity> DeleteEntityAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>;
    Task<TEntity> DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<Guid>;

    Task BulkInsertEntityAsync<TEntity, TKey>(List<TEntity> entities) where TEntity : class, IEntity<TKey>;
    Task BulkInsertEntityAsync<TEntity>(List<TEntity> entities) where TEntity : class, IEntity<Guid>;

    Task BulkUpdateEntityAsync<TEntity, TKey>(List<TEntity> entities) where TEntity : class, IEntity<TKey>;
    Task BulkUpdateEntityAsync<TEntity>(List<TEntity> entities) where TEntity : class, IEntity<Guid>;

    Task BulkDeleteEntityAsync<TEntity, TKey>(List<TEntity> entities) where TEntity : class, IEntity<TKey>;
    Task BulkDeleteEntityAsync<TEntity>(List<TEntity> entities) where TEntity : class, IEntity<Guid>;

    Task<List<TEntity>> QueryAsync<TEntity>(string query, Dictionary<string, object> parameters);
    ITransaction BeginTransaction();
    int SaveChanges();
    Task MigrateAsync();
}