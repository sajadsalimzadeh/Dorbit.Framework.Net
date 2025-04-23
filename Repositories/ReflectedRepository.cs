using System;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories;

public class ReflectedRepository(object repository)
{
    private readonly Type _type = repository.GetType();

    public IQueryable<IEntity> Set()
    {
        var setMethodInfo = repository?.GetType().GetMethod("Set") ?? throw new Exception("Method Set() not found");
        return ((IQueryable)setMethodInfo.Invoke(repository, [true]))?.Cast<IEntity>() ?? throw new Exception("Query not found in entity repository");
    }

    public async Task UpdateAsync(object entity)
    {
        await ((Task)_type.GetMethod("UpdateAsync")?.Invoke(repository, [entity]))!;
    }

    public async Task InsertAsync(object entity)
    {
        await ((Task)_type.GetMethod("InsertAsync")?.Invoke(repository, [entity]))!;
    }
}