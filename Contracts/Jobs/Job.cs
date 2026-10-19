using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Services;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Contracts.Jobs;

public class Job
{
    public enum AuditLogType
    {
        Create = 0,
        Start = 1,
        Cancel = 2,
        Pause = 3,
        Resume = 4,
    }

    public class AuditLog
    {
        public AuditLogType Type { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; }
        public string UserName { get; set; }

        public AuditLog(AuditLogType type, IUserDto userDto)
        {
            Type = type;
            if (userDto is not null)
            {
                UserId = userDto.GetId().ToString();
                UserName = userDto.GetUsername();
            }
        }

        public AuditLog(AuditLogType type, string userId, string userName)
        {
            Type = type;
            UserId = userId;
            UserName = userName;
        }
    }


    private JobStatus _status = JobStatus.Draft;
    private readonly Lock _startLock = new();
    private Task _startTask;
    private CancellationToken _executionToken;
    private double _progress;
    private readonly JobService _jobService;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Guid Id { get; set; } = Guid.NewGuid();
    public Exception Exception { get; private set; }
    public string Step { get; set; }
    public string Name { get; }
    public Func<Job, Task> Action { get; }
    public List<JobLog> Logs { get; } = [];
    public List<AuditLog> AuditLogs { get; } = [];
    public JobLogger Logger { get; }
    public bool Pausable { get; set; }
    public string DownloadFilename { get; set; }
    public Func<Stream> Download { get; set; }
    public CancellationToken CancellationToken => _executionToken;

    public DateTime? StartTime { get; private set; }
    public DateTime? CancelTime { get; private set; }
    public DateTime? EndTime { get; private set; }


    public event EventHandler<JobStatus> OnStatusChange;
    public event EventHandler<double> OnProgress;

    public JobStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnStatusChange?.Invoke(this, _status);
        }
    }

    public double Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            if (_progress > 100) _progress = 100;
            OnProgress?.Invoke(this, _progress);
        }
    }

    public Job(string name, Func<Job, Task> action, JobService jobService)
    {
        Name = name;
        Action = action;   
        Logger = new JobLogger(this);
        _jobService = jobService;
        _executionToken = _cancellationTokenSource.Token;
    }

    public void Start()
    {
        lock (_startLock)
        {
            _startTask ??= StartAndWaitAsync();
        }
    }

    public Task StartAndWaitAsync()
    {
        return _jobService?.RunJobAsync(this, RunAsync) ?? Task.Run(RunAsync);
    }

    private async Task RunAsync()
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            _cancellationTokenSource.Token, _jobService?.StoppingToken ?? App.StoppingToken);
        _executionToken = linked.Token;
        try
        {
            CancellationToken.ThrowIfCancellationRequested();
            StartTime = DateTime.UtcNow;
            Status = JobStatus.Running;
            await Action.Invoke(this);
        }
        catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            CancelTime ??= DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Exception = ex;
            Logger.LogError($"{ex.Message} {ex.InnerException?.Message}");
        }
        finally
        {
            EndTime = DateTime.UtcNow;
            Status = (Logs.Any(x => x.Level == LogLevel.Error || x.Level == LogLevel.Critical) ? JobStatus.FinishError : JobStatus.Finish);
            try
            {
                await UpdateStatusAsync();
            }
            catch (OperationCanceledException) when (linked.IsCancellationRequested) { }
            finally
            {
                _executionToken = _cancellationTokenSource.Token;
            }
        }
    }

    public void Cancel()
    {
        if (_cancellationTokenSource.IsCancellationRequested) return;
        _cancellationTokenSource.Cancel();
        CancelTime = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (!Pausable) return;
        Status = JobStatus.Pause;
    }

    public void Resume()
    {
        if (!Pausable) return;
        Status = JobStatus.Running;
    }

    public Task UpdateStatusAsync()
    {
        return _jobService?.UpdateJobAsync(this) ?? Task.CompletedTask;
    }
    
    public Task UpdateStatusAsync(string step)
    {
        Step = step;
        return _jobService?.UpdateJobAsync(this) ?? Task.CompletedTask;
    }
    
    public Task UpdateStatusAsync(double progress)
    {
        Progress = progress;
        return _jobService?.UpdateJobAsync(this) ?? Task.CompletedTask;
    }
    
    public Task UpdateStatusAsync(string step, double progress)
    {
        Step = step;
        Progress = progress;
        return _jobService?.UpdateJobAsync(this) ?? Task.CompletedTask;
    }
}
