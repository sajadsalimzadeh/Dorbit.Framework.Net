using System;
using System.Linq;
using System.Threading;
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

    public async Task UpdateAsync(object entity, CancellationToken cancellationToken = default)
    {
        var methodInfo = _type.GetMethods().FirstOrDefault(x =>
            x.Name == nameof(BaseWriteRepository<Entity>.UpdateAsync) &&
            x.GetParameters().FirstOrDefault()?.ParameterType.IsAssignableTo(typeof(IEntity)) == true
        );
        await ((Task)methodInfo?.Invoke(repository, [entity, cancellationToken]))!;
    }

    public async Task InsertAsync(object entity, CancellationToken cancellationToken = default)
    {
        var methodInfo = _type.GetMethods().FirstOrDefault(x =>
            x.Name == nameof(BaseWriteRepository<Entity>.InsertAsync) &&
            x.GetParameters().FirstOrDefault()?.ParameterType.IsAssignableTo(typeof(IEntity)) == true
        );
        await ((Task)methodInfo?.Invoke(repository, [entity, cancellationToken]))!;
    }
}