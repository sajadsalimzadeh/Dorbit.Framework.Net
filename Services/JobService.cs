using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Jobs;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JobService
{
    private readonly IMemoryCache _memoryCache;

    public JobService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<List<Job>> GetAllAsync()
    {
        const string key = $"{nameof(JobService)}-Jobs";
        if (!_memoryCache.TryGetValue<List<Job>>(key, out var jobs)) _memoryCache.Set(key, jobs = []);
        return Task.FromResult(jobs);
    }

    public async Task<Job> GetAsync(Guid id)
    {
        var jobs = await GetAllAsync();
        return jobs.FirstOrDefault(x => x.Id == id);
    }

    public async Task<Job> Create(JobCreateRequest request)
    {
        var jobs = await GetAllAsync();
        var job = new Job
        {
            Name = request.Name
        };
        jobs.Add(job);
        return job;
    }
}