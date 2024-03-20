using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Database.Abstractions;

public interface IDbContext
{
    IServiceProvider ServiceProvider { get; }
    bool AutoExcludeDeleted { get; set; }

    IQueryable<T> DbSet<T>() where T : class, IEntity;
    IQueryable<T> DbSet<T>(bool excludeDeleted) where T : class, IEntity;
    Task<T> InsertEntityAsync<T>(T model) where T : class, IEntity;
    Task<T> UpdateEntityAsync<T>(T model) where T : class, IEntity;
    Task<T> DeleteEntityAsync<T>(T model) where T : class, IEntity;
    Task BulkInsertEntityAsync<T>(List<T> items) where T : class, IEntity;
    Task BulkUpdateEntityAsync<T>(List<T> items) where T : class, IEntity;
    Task BulkDeleteEntityAsync<T>(List<T> items) where T : class, IEntity;
    Task<List<T>> QueryAsync<T>(string query, Dictionary<string, object> parameters);
    ITransaction BeginTransaction();
    int SaveChanges();
    Task MigrateAsync();
}