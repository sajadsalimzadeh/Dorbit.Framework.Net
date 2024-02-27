using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JobService(IMemoryCache memoryCache, IUserResolver userResolver)
{
    public Task<List<Job>> GetAllAsync()
    {
        const string key = $"{nameof(JobService)}-Jobs";
        if (!memoryCache.TryGetValue<List<Job>>(key, out var jobs)) memoryCache.Set(key, jobs = []);
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
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Create, userResolver.User));
        jobs.Add(job);
        return job;
    }
}