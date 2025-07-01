using System;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Entities;
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
        var methodInfo = _type.GetMethods().FirstOrDefault(x => x.Name == nameof(BaseWriteRepository<Entity>.UpdateAsync) && x.GetParameters().Length == 1);
        await ((Task)methodInfo?.Invoke(repository, [entity]))!;
    }

    public async Task InsertAsync(object entity)
    {
        var methodInfo = _type.GetMethods().FirstOrDefault(x => x.Name == nameof(BaseWriteRepository<Entity>.InsertAsync) && x.GetParameters().Length == 1);
        await ((Task)methodInfo?.Invoke(repository, [entity]))!;
    }
}