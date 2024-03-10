using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;
using MessagePack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Repositories;

public abstract class DistributedCacheRepository<T> where T : IEntity
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDistributedCache _cache;

    protected DistributedCacheRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cache = serviceProvider.GetService<IDistributedCache>();
    }

    public async Task<T> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(id.ToString(), cancellationToken);
        return (bytes is not null ? MessagePackSerializer.Deserialize<T>(bytes, cancellationToken: cancellationToken) : default);   
    }

    public async Task<List<T>> GetAllAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        foreach (var id in ids)
        {
            result.Add(await GetAsync(id, cancellationToken));
        }
        return result;
    }

    public async Task<T> SetAsync(T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        var bytes = MessagePackSerializer.Serialize(value, cancellationToken: cancellationToken);
        await _cache.SetAsync(value.Id.ToString(), bytes, options, cancellationToken);
        return value;
    }
}