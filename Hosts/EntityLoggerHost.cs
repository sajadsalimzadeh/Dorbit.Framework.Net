using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Loggers;
using Dorbit.Framework.Database;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Entities.Abstractions;
using EFCore.BulkExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class EntityLoggerHost(IServiceProvider serviceProvider) : BaseHostInterval(serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private static readonly ConcurrentQueue<LogRequest> Requests = new();

    public EntityLoggerHost Add(LogRequest request)
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
                foreach (var property in type.GetProperties().Where(x => x.GetCustomAttribute<EntityLogAttribute>() is not null))
                {
                    if (!property.PropertyType.IsPrimitive) continue;
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
                    Data = JsonSerializer.Serialize(diff, new JsonSerializerOptions()
                    {
                        MaxDepth = 1
                    }),
                    Action = request.Action,
                    CreationTime = DateTime.UtcNow,
                    CreatorId = request.User?.GetId()?.ToString(),
                    CreatorName = request.User?.GetUsername(),
                };
                logs.Add(entity);
            }

            if (logs.Count > 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<FrameworkDbContext>();
                await dbContext.BulkInsertAsync(logs, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, ex.Message);
        }
    }
}