using Devor.Framework.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devor.Framework.Database.Abstractions
{
    public interface IDbContext
    {
        IServiceProvider ServiceProvider { get; }

        IDbContext AutoExcludeDeletedEnable();
        IDbContext AutoExcludeDeletedDisable();

        IQueryable<T> DbSet<T>() where T : class, IEntity;
        IQueryable<T> DbSet<T>(bool excludeDeleted) where T : class, IEntity;
        T InsertEntity<T>(T model) where T : class, IEntity;
        T UpdateEntity<T>(T model) where T : class, IEntity;
        T RemoveEntity<T>(T model) where T : class, IEntity;

        ITransaction BeginTransaction();
        int SaveChanges();
        IEnumerable<Type> GetLookupEntities();

        void Migrate();
        void UnTrackhUnChangeEntries<T>(T model) where T : class, IEntity;
        List<T> Query<T>(string query, Dictionary<string, object> parameters);
    }
}
