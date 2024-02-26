using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

public class JobsController(JobService jobService) : BaseController
{
    [HttpGet]
    public async Task<QueryResult<List<JobDto>>> GetAllAsync()
    {
        var jobs = await jobService.GetAllAsync();
        return jobs.MapTo<List<JobDto>>().ToQueryResult();
    }
    
    [HttpGet("{id:guid}/Progress")]
    public async IAsyncEnumerable<double> GetProgressAsync(Guid id, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        var job = await jobService.GetAsync(id);
        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(50, cancellationToken);
            yield return job.Progress;
        }
    }

    [HttpGet("{id:guid}/Logs")]
    public async Task<QueryResult<List<JobLogDto>>> GetAllLogsAsync(Guid id)
    {
        var job = await jobService.GetAsync(id);
        return job.Logs.MapTo<List<JobLogDto>>().ToQueryResult();
    }

    [HttpPost("{id:guid}/Cancel")]
    public async Task<Job> CancelAsync(Guid id)
    {
        var job = await jobService.GetAsync(id);
        job.Cancel();
        return job;
    }
}