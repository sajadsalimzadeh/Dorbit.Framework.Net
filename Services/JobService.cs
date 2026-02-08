using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Hubs;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class JobService : IJobHub
{
    private readonly ConcurrentQueue<Job> _queue = new();
    private readonly IMemoryCache _memoryCache;
    private readonly HubManager _hubManager;

    public JobService(IMemoryCache memoryCache, HubManager hubManager, ILogger logger)
    {
        _memoryCache = memoryCache;
        _hubManager = hubManager;

        new Thread(JobQueueWorker).Start();
        
        async void JobQueueWorker()
        {
            while (!App.StoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var job))
                    {
                        await job.StartAndWaitAsync();
                    }
                }
                catch(Exception ex)
                {
                    logger.Error(ex, $"{ex.Message} {ex.StackTrace}");
                }

                Thread.Sleep(500);
            }
        }
    }

    public List<Job> GetAll()
    {
        const string key = $"{nameof(JobService)}-Jobs";
        if (!_memoryCache.TryGetValue<List<Job>>(key, out var jobs)) _memoryCache.Set(key, jobs = []);
        return jobs;
    }

    public Job GetAsync(Guid id)
    {
        var jobs = GetAll();
        return jobs.FirstOrDefault(x => x.Id == id);
    }

    public Job Create(JobCreateRequest request)
    {
        var jobs = GetAll();
        var job = new Job(request.Name, request.Action, this);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Create, request.User));
        jobs.Add(job);
        return job;
    }

    public Job Enqueue(Job job)
    {
        _queue.Enqueue(job);
        return job;
    }

    public async Task UpdateAllJobAsync()
    {
        var jobs = GetAll();
        foreach (var job in jobs)
        {
            await UpdateJobAsync(job);
        }
    }

    public async Task UpdateJobAsync(Job job)
    {
        await _hubManager.HubContext.Clients.Group(BaseHub.GroupJobStatus).SendCoreAsync(BaseHub.GroupJobStatus, [
            new JobStatusDto()
            {
                Id = job.Id,
                Step = job.Step,
                Progress = job.Progress,
                EndTime = job.EndTime,
                CancelTime = job.CancelTime,
                Status = job.Status,
            }
        ]);
    }
}