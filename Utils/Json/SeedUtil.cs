using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Dorbit.Framework.Utils.Json;

public static class SeedUtil
{
    public static async Task SeedAsync<T>(this IWriterRepository<T> repository, string filename, Func<T, object> predicate,
        Action<T> beforeInsert = default)
        where T : class, IEntity
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
        var content = await File.ReadAllTextAsync(path);
        var items = JsonConvert.DeserializeObject<List<T>>(content);
        var existsItems = (await repository.Set().ToListAsync()).Select(predicate);
        using var transaction = repository.DbContext.BeginTransaction();
        foreach (var entity in items.Where(x => !existsItems.Contains(predicate(x))))
        {
            if (beforeInsert is not null) beforeInsert(entity);
            await repository.InsertAsync(entity);
        }

        transaction.Commit();
    }
}