using System.Reflection;
using Dorbit.Attributes;
using Dorbit.Database;
using Dorbit.Entities;
using Dorbit.Entities.Abstractions;
using Dorbit.Enums;
using Dorbit.Models.Loggers;
using Dorbit.Services.Abstractions;
using Dorbit.Utils.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Hosts
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class LoggerHost : IStartup
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThreadService _threadService;
        private static readonly Queue<LogRequest> Requests = new();
        static Queue<EntityLog> _logQueue = new();

        public LoggerHost(IServiceProvider serviceProvider, IThreadService threadService)
        {
            _serviceProvider = serviceProvider;
            _threadService = threadService;
        }

        public LoggerHost Add(LogRequest request)
        {
            lock (Requests)
            {
                Requests.Enqueue(request);
            }
            return this;
        }

        public LoggerHost Add(EntityLog item)
        {
            _logQueue.Enqueue(item);
            return this;
        }

        public void Run()
        {
            new Thread(() =>
            {
                while (_threadService.MainThread?.IsAlive != false)
                {
                    try
                    {
                        var sp = _serviceProvider.CreateScope().ServiceProvider;
                        var loggerService = sp.GetService<ILoggerService>();
                        try
                        {
                            lock (Requests)
                            {
                                while (Requests.Count > 0)
                                {
                                    LogRequest request;
                                    lock (Requests)
                                    {
                                        request = Requests.Dequeue();
                                    }
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
                                        ReferenceId = request.NewObj.Id,
                                        Data = JsonConverterWrapper.SerializeObject(diff, 1),
                                        Action = request.Action,
                                        CreationTime = DateTime.Now,
                                        CreatorId = request.User?.Id,
                                        CreatorName = request.User?.Name,
                                    };
                                    Add(entity);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            loggerService.LogError(ex);
                        }
                    }
                    catch { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            }).Start();
            new Thread(() =>
            {
                while (_threadService.MainThread?.IsAlive != false)
                {
                    try
                    {
                        var sp = _serviceProvider.CreateScope().ServiceProvider;
                        var loggerService = sp.GetService<ILoggerService>();
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            lock (_logQueue)
                            {
                                var dbContext = scope.ServiceProvider.GetService<LogDbContext>();
                                while (_logQueue.Count > 0)
                                {
                                    var item = _logQueue.Dequeue();
                                    dbContext.Set<EntityLog>().Add(item);
                                }
                                dbContext.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            loggerService.LogError(ex);
                        }
                    }
                    catch { }
                    finally
                    {
                        Thread.Sleep(10000);
                    }
                }
            }).Start();
        }
    }
}
