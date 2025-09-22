using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Database.Abstractions;

public interface IDbContext : IDbContextMigrator
{
    IServiceProvider ServiceProvider { get; }
    bool AutoExcludeDeleted { get; set; }
    DatabaseProviderType ProviderType { get; }

    IQueryable<TEntity> DbSet<TEntity, TKey>(bool? excludeDeleted = null) where TEntity : class, IEntity<TKey>;
    IQueryable<TEntity> DbSet<TEntity>(bool? excludeDeleted = null) where TEntity : class, IEntity<Guid>;

    Task<TEntity> InsertEntityAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>;
    Task<TEntity> InsertEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>;

    Task<TEntity> UpdateEntityAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>;
    Task<TEntity> UpdateEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>;

    Task<TEntity> DeleteEntityAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>;
    Task<TEntity> DeleteEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>;

    Task BulkInsertEntityAsync<TEntity, TKey>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>;
    Task BulkInsertEntityAsync<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>;

    Task BulkUpdateEntityAsync<TEntity, TKey>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>;
    Task BulkUpdateEntityAsync<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>;

    Task BulkDeleteEntityAsync<TEntity, TKey>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>;
    Task BulkDeleteEntityAsync<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>;

    Task<int> ExecuteCommandAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    Task<List<TEntity>> ExecuteQueryAsync<TEntity>(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    ITransaction BeginTransaction();
    int SaveChanges();
    Task MigrateAsync(CancellationToken cancellationToken = default);
}