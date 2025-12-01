using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Hubs;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class JobService(IMemoryCache memoryCache, HubManager hubManager) : IJobHub
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
        var job = new Job(this)
        {
            Name = request.Name
        };
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Create, request.User));
        jobs.Add(job);
        return job;
    }
    
    public async Task UpdateAllJobAsync()
    {
        var jobs = await GetAllAsync();
        foreach (var job in jobs)
        {
            await UpdateJobAsync(job);
        }
    }

    public async Task UpdateJobAsync(Job job)
    {
        await hubManager.HubContext.Clients.Group(BaseHub.GroupJobStatus).SendCoreAsync(BaseHub.GroupJobStatus, [new JobStatusDto()
        {
            Id = job.Id,
            Step = job.Step,
            Progress = job.Progress,
            EndTime = job.EndTime,
            CancelTime = job.CancelTime,
            Status = job.Status,
        }]);
    }
}