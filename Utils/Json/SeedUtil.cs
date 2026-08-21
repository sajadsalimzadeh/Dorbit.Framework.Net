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
    public static async Task SeedAsync<TEntity, TKey, TUniqueKey>(this IWriterRepository<TEntity, TKey> repository, string filename,
        Func<TEntity, TUniqueKey> keyFunc,
        Func<TEntity, Task> beforeInsertAction = null,
        Action<TEntity, TEntity> update = null
    )
        where TEntity : class, IEntity<TKey>
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
        var content = await File.ReadAllTextAsync(path);
        var items = JsonSerializer.Deserialize<List<TEntity>>(content, JsonSerializerOptions.Web);
        await repository.SeedAsync(items, keyFunc, beforeInsertAction, update);
    }

    public static async Task SeedAsync<TEntity, TKey, TUniqueKey>(this IWriterRepository<TEntity, TKey> repository, List<TEntity> items,
        Func<TEntity, TUniqueKey> keyFunc,
        Func<TEntity, Task> beforeInsertAction = null,
        Action<TEntity, TEntity> update = null)
        where TEntity : class, IEntity<TKey>
    {
        var existsItems = await repository.Set(false).ToListAsync();

        var insertItems = new List<TEntity>();
        var updateItems = new List<TEntity>();
        foreach (var item in items.Where(x => !existsItems.Contains(x)))
        {
            if (beforeInsertAction is not null) await beforeInsertAction(item);
            var existsItem = existsItems.FirstOrDefault(x => keyFunc(item).Equals(keyFunc(x)));
            if (existsItem != null)
            {
                if (update is not null)
                {
                    update(existsItem, item);
                    updateItems.Add(item);
                }
            }
            else
            {
                insertItems.Add(item);
            }
        }

        if (insertItems.Any()) await repository.BulkInsertAsync(insertItems);
        if (updateItems.Any()) await repository.BulkUpdateAsync(updateItems);
    }
}