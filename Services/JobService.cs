using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Hubs;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class JobService : BackgroundService, IJobHub
{
    private readonly ConcurrentQueue<Job> _queue = new();
    private readonly IMemoryCache _memoryCache;
    private readonly HubManager _hubManager;
    private readonly CancellationTokenSource _shutdown;
    private readonly CancellationToken _shutdownToken;
    private readonly ILogger _logger;
    private readonly object _runningLock = new();
    private readonly Dictionary<Guid, Task> _runningJobs = new();
    private int _disposed;

    internal CancellationToken StoppingToken => _shutdownToken;

    public JobService(IMemoryCache memoryCache, HubManager hubManager, ILogger logger,
        IHostApplicationLifetime lifetime = null)
    {
        _memoryCache = memoryCache;
        _hubManager = hubManager;

        _logger = logger;
        _shutdown = CancellationTokenSource.CreateLinkedTokenSource(lifetime?.ApplicationStopping ?? App.StoppingToken);
        _shutdownToken = _shutdown.Token;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _shutdownToken);
        try
        {
            while (!linked.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var job))
                    {
                        linked.Token.ThrowIfCancellationRequested();
                        await job.StartAndWaitAsync();
                    }
                }
                catch (OperationCanceledException) when (linked.IsCancellationRequested)
                {
                    break;
                }
                catch(Exception ex)
                {
                    _logger?.Error(ex, "Job queue execution failed");
                }

                await Task.Delay(500, linked.Token);
            }
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested) { }
    }

    internal Task RunJobAsync(Job job, Func<Task> action)
    {
        lock (_runningLock)
        {
            _shutdownToken.ThrowIfCancellationRequested();
            if (_runningJobs.TryGetValue(job.Id, out var running)) return running;
            var task = Task.Run(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "Job execution failed");
                }
                finally
                {
                    lock (_runningLock) _runningJobs.Remove(job.Id);
                }
            });
            _runningJobs.Add(job.Id, task);
            return task;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdown.Cancel();
        await base.StopAsync(cancellationToken);
        Task[] running;
        lock (_runningLock) running = _runningJobs.Values.ToArray();
        await Task.WhenAll(running).WaitAsync(cancellationToken);
    }

    public override void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _shutdown.Cancel();
        base.Dispose();
        _shutdown.Dispose();
    }

    public List<Job> GetAll()
    {
        const string key = $"{nameof(JobService)}-Jobs";
        return _memoryCache.GetValueWithLock(key, () => new List<Job>(), TimeSpan.FromDays(1));
    }

    public Job GetAsync(Guid id)
    {
        var jobs = GetAll();
        return jobs.FirstOrDefault(x => x.Id == id);
    }

    public Job Create(JobCreateRequest request)
    {
        _shutdownToken.ThrowIfCancellationRequested();
        var jobs = GetAll();
        var job = new Job(request.Name, request.Action, this);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Create, request.User));
        jobs.Add(job);
        return job;
    }

    public Job Enqueue(Job job)
    {
        _shutdownToken.ThrowIfCancellationRequested();
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
        _shutdownToken.ThrowIfCancellationRequested();
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
        ], _shutdownToken);
    }
}
