﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Abstractions;
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
    private Thread _thread;
    private double _progress;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Semaphore _semaphore = new(0, 1);

    public Guid Id { get; set; } = Guid.NewGuid();
    public Exception Exception { get; private set; }
    public string Name { get; set; }
    public List<JobLog> Logs { get; } = [];
    public List<AuditLog> AuditLogs { get; } = [];
    public JobLogger Logger { get; }
    public bool Pausable { get; set; }
    public string DownloadFilename { get; set; }
    public Func<Stream> Download { get; set; }

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

    public Job()
    {
        Logger = new JobLogger(this);
    }

    public void Start(Func<CancellationToken, Task> task)
    {
        if (_thread is not null) return;

        _thread = new Thread(ThreadStart);
        _thread.Start();
        StartTime = DateTime.UtcNow;
        return;

        async void ThreadStart()
        {
            try
            {
                Status = JobStatus.Running;
                await task(_cancellationTokenSource.Token);
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
                _semaphore.Release();
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

    public Task Wait()
    {
        _semaphore.WaitOne();
        return Task.CompletedTask;
    }
}