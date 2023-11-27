using System.Collections.Concurrent;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Database;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Enums;
using Dorbit.Framework.Models.Loggers;
using Dorbit.Framework.Utils.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class LoggerHost : BaseHost
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentQueue<LogRequest> Requests = new();

    public LoggerHost(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public LoggerHost Add(LogRequest request)
    {
        Requests.Enqueue(request);

        return this;
    }

    protected override int IntervalInSec { get; } = 3;

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
                        else if (oldValue?.Equals(newValue) == true) continue;
                    }

                    if (request.NewObj is IModificationTime && property.Name == nameof(IModificationTime.ModificationTime)) continue;
                    diff[property.Name] = newValue;
                }

                if (diff.Count == 0) continue;
                var entity = new EntityLog()
                {
                    Module = request.Module,
                    EntityType = request.NewObj.GetType().Name,
                    ReferenceId = request.NewObj.Id.ToString(),
                    Data = JsonConverterWrapper.SerializeObject(diff, 1),
                    Action = request.Action,
                    CreationTime = DateTime.Now,
                    CreatorId = request.User?.Id,
                    CreatorName = request.User?.Name,
                };
                logs.Add(entity);
            }

            if (logs.Count > 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<LogDbContext>();
                var set = dbContext.Set<EntityLog>();
                await set.AddRangeAsync(logs);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogError(ex);
        }
    }
}