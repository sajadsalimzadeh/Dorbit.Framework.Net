using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Loggers;
using Dorbit.Framework.Database;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Utils.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class LoggerHostInterval : BaseHostInterval
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentQueue<LogRequest> Requests = new();

    public LoggerHostInterval(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public LoggerHostInterval Add(LogRequest request)
    {
        Requests.Enqueue(request);

        return this;
    }

    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(30);

    protected override async Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var logs = new List<EntityLog>();
            while (Requests.TryDequeue(out var request))
            {
                var type = request.NewObj.GetType();
                var diff = new Dictionary<string, object>();
                foreach (var property in type.GetProperties())
                {
                    if (!property.PropertyType.IsPrimitive) continue;
                    if (property.GetCustomAttribute<IgnoreLoggingAttribute>() is not null) continue;
                    var newValue = property.GetValue(request.NewObj);
                    if (request.Action == LogAction.Update)
                    {
                        var oldValue = property.GetValue(request.OldObj);
                        if (oldValue is null && newValue is null) continue;
                        if (oldValue?.Equals(newValue) == true) continue;
                    }

                    if (request.NewObj is IModificationTime && property.Name == nameof(IModificationTime.ModificationTime)) continue;
                    diff[property.Name] = newValue;
                }

                if (diff.Count == 0) continue;
                var entity = new EntityLog
                {
                    Module = request.Module,
                    EntityType = request.NewObj.GetType().Name,
                    ReferenceId = type.GetProperty("Id")?.GetValue(request.NewObj)?.ToString(),
                    Data = JsonConverterWrapper.SerializeObject(diff, 1),
                    Action = request.Action,
                    CreationTime = DateTime.UtcNow,
                    CreatorId = request.User?.Id?.ToString(),
                    CreatorName = request.User?.Username,
                };
                logs.Add(entity);
            }

            if (logs.Count > 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<FrameworkDbContext>();
                var set = dbContext.Set<EntityLog>();
                await set.AddRangeAsync(logs, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, ex.Message);
        }
    }
}