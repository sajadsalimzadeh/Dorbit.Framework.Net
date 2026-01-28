using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class JobsController(JobService jobService) : BaseController
{
    [HttpGet]
    public QueryResult<List<JobDto>> GetAll()
    {
        var jobs = jobService.GetAll();
        return jobs.MapTo<List<JobDto>>().ToQueryResult();
    }

    [HttpGet("{id:guid}/Logs")]
    public QueryResult<List<JobLogDto>> GetAllLogs(Guid id, LogLevel? logLevel)
    {
        var job = jobService.GetAsync(id);
        var logs = job.Logs;
        if (logLevel.HasValue) logs = logs.Where(x => x.Level == logLevel.Value).ToList();
        return logs.MapTo<List<JobLogDto>>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Cancel")]
    public QueryResult<JobDto> CancelAsync(Guid id)
    {
        var job = jobService.GetAsync(id);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Cancel, UserResolver.User));
        job.Cancel();
        return job.MapTo<JobDto>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Pause")]
    public QueryResult<JobDto> PauseAsync(Guid id)
    {
        var job = jobService.GetAsync(id);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Pause, UserResolver.User));
        job.Pause();
        return job.MapTo<JobDto>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Resume")]
    public QueryResult<JobDto> ResumeAsync(Guid id)
    {
        var job = jobService.GetAsync(id);
        job.AuditLogs.Add(new Job.AuditLog(Job.AuditLogType.Resume, UserResolver.User));
        job.Resume();
        return job.MapTo<JobDto>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Download")]
    public FileStreamResult DownloadAsync(Guid id)
    {
        var job = jobService.GetAsync(id);
        return File(job.Download(), "application/octet-stream");
    }
}