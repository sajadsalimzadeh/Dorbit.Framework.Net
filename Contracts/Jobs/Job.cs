using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dorbit.Framework.Contracts.Jobs;

public class Job
{
    public Guid Id { get; } = Guid.NewGuid();
    private JobStatus _status = JobStatus.Draft;
    private Thread _thread;
    private double _progress = 0;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

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

    public string Name { get; set; }
    public List<JobLog> Logs { get; } = [];
    public JobLogger Logger { get; }

    public event EventHandler<JobStatus> OnStatusChange;
    public event EventHandler<double> OnProgress;

    public Job()
    {
        Logger = new JobLogger(this);
    }

    public void Start(Func<CancellationToken, Task> task)
    {
        if (_thread is not null) return;
        _thread = new Thread(ThreadStart);
        _thread.Start();
        return;

        async void ThreadStart()
        {
            try
            {
                Status = JobStatus.Running;
                await task(_cancellationTokenSource.Token);
                Status = JobStatus.Success;
            }
            catch (Exception ex)
            {
                Status = JobStatus.Failed;
            }
        }
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }
}