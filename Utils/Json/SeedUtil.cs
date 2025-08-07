using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Utils.Json;

public static class SeedUtil
{
    public static async Task SeedAsync<TEntity, TKey>(this IWriterRepository<TEntity, TKey> repository, string filename,
        Func<TEntity, Task> beforeInsertAction = default,
        Func<TEntity, TEntity, bool> ignorePredicate = default)
        where TEntity : class, IEntity<TKey>
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
        var content = await File.ReadAllTextAsync(path);
        var items = JsonSerializer.Deserialize<List<TEntity>>(content, JsonSerializerOptions.Web);
        await repository.SeedAsync(items, beforeInsertAction, ignorePredicate);
    }

    public static async Task SeedAsync<TEntity, TKey>(this IWriterRepository<TEntity, TKey> repository, List<TEntity> items,
        Func<TEntity, Task> beforeInsertAction = default,
        Func<TEntity, TEntity, bool> ignorePredicate = default)
        where TEntity : class, IEntity<TKey>
    {
        var existsItems = await repository.Set(false).ToListAsync();
        if (ignorePredicate is not null) items = items.Where(x => !existsItems.Any(y => ignorePredicate(x, y))).ToList();
        var insertItems = new List<TEntity>();
        foreach (var entity in items.Where(x => !existsItems.Contains(x)))
        {
            if (beforeInsertAction is not null) await beforeInsertAction(entity);
            insertItems.Add(entity);
        }

        await repository.BulkInsertAsync(insertItems);
    }
}