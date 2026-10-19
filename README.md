Development Orbit

## Hosted workers and shutdown

The framework targets .NET 10. `BaseHost` and `BaseHostInterval` return the task representing their complete work to `BackgroundService`; they do not create foreground threads. The `isConcurrent` constructor argument remains for source compatibility. Interval workers keep their initial delay and stop before invoking work when cancellation interrupts that delay. Scoped dependencies are released with asynchronous scope disposal. Cancellation caused by shutdown is treated as normal completion; other errors retain the existing logging behavior.

`JobService` is a hosted singleton, registered as `IHostedService` by the existing service scanner. Creating it no longer starts a worker in its constructor. Queue execution is sequential. Jobs started through `Job.Start()` or `StartAndWaitAsync()` with this service are also tracked, and use a token linked to job cancellation and service/application shutdown while running. Stop prevents new work, cancels active jobs, and waits for their cleanup within the host's shutdown cancellation budget. Existing job status values and SignalR method names are preserved; cancellation records `CancelTime` without adding a new status. Job actions must observe `job.CancellationToken`; synchronous/native work that ignores cancellation cannot be forcibly interrupted safely by this mechanism.

`BuildDorbit` publishes the actual host's service provider through `App.ServiceProvider`, so static access and the host share singleton instances and disposal ownership. The service scanner no longer builds an unused temporary provider. `HttpHelper` combines request, helper-property, and application-stopping cancellation for sending and generic response-body reads. A failed generic response read disposes the response. Successful responses remain owned by the caller. Low-level `SendAsync(HttpRequestMessage, ...)` callers still own response streaming and must pass appropriate cancellation to their body reads.

`TaskUtil.Delay` retains its historical cancellation-swallowing behavior for existing callers. The interval host uses `Task.Delay` directly, and queue workers explicitly exit when stopped. Shared blocking cache APIs and caller-owned linked-token extensions are outside these shutdown fixes.

Run the isolated shutdown regression executable from this repository root:

```powershell
dotnet run --project Tests/ShutdownRegression/ShutdownRegression.csproj
```

It checks worker task tracking, idle and active interval cancellation, asynchronous scope disposal, queued/manual job shutdown, the shutdown wait budget, HTTP body cancellation/disposal, and host ownership of static DI resources. It uses fake HTTP handlers and disposable local hosts; it does not start a real application, migrate databases, access hardware, or contact external services.
