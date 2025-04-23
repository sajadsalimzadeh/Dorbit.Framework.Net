using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
public class JobsController(JobService jobService) : BaseController
{
    [HttpGet]
    public async Task<QueryResult<List<JobDto>>> GetAllAsync()
    {
        var jobs = await jobService.GetAllAsync();
        return jobs.MapTo<List<JobDto>>().ToQueryResult();
    }

    [HttpGet("Watch")]
    public async IAsyncEnumerable<JobStatusDto> WatchAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        var jobs = await jobService.GetAllAsync();
        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(250, cancellationToken);
            foreach (var statusDto in jobs.Select(x => new JobStatusDto() { Id = x.Id, Progress = x.Progress, Status = x.Status, }))
            {
                yield return statusDto;
            }
        }
    }

    [HttpGet("{id:guid}/Logs")]
    public async Task<QueryResult<List<JobLogDto>>> GetAllLogsAsync(Guid id, LogLevel? logLevel)
    {
        var job = await jobService.GetAsync(id);
        var logs = job.Logs;
        if (logLevel.HasValue) logs = logs.Where(x => x.Level == logLevel.Value).ToList();
        return logs.MapTo<List<JobLogDto>>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Cancel")]
    public async Task<QueryResult<JobDto>> CancelAsync(Guid id)
    {
        var job = await jobService.GetAsync(id);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Cancel, UserResolver.User));
        job.Cancel();
        return job.MapTo<JobDto>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Pause")]
    public async Task<QueryResult<JobDto>> PauseAsync(Guid id)
    {
        var job = await jobService.GetAsync(id);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Pause, UserResolver.User));
        job.Pause();
        return job.MapTo<JobDto>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Resume")]
    public async Task<QueryResult<JobDto>> ResumeAsync(Guid id)
    {
        var job = await jobService.GetAsync(id);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Resume, UserResolver.User));
        job.Resume();
        return job.MapTo<JobDto>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Download")]
    public async Task<FileStreamResult> DownloadAsync(Guid id)
    {
        var job = await jobService.GetAsync(id);
        return File(job.Download(), "application/octet-stream");
    }
}