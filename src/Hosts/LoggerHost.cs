using Devor.Framework.Attributes;
using Devor.Framework.Database.Abstractions;
using Devor.Framework.Entities;
using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Enums;
using Devor.Framework.Extensions;
using Devor.Framework.Services.Abstractions;
using Devor.Framework.Utils.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Devor.Framework.Database;
using Devor.Framework.Models.Loggers;

namespace Devor.Framework.Hosts
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class LoggerHost : IStartup
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IThreadService threadService;
        private static readonly Queue<LogRequest> requests = new Queue<LogRequest>();
        static Queue<EntityLog> logQueue = new Queue<EntityLog>();

        public LoggerHost(IServiceProvider serviceProvider, IThreadService threadService)
        {
            this.serviceProvider = serviceProvider;
            this.threadService = threadService;
        }

        public LoggerHost Add(LogRequest request)
        {
            lock (requests)
            {
                requests.Enqueue(request);
            }
            return this;
        }

        public LoggerHost Add(EntityLog item)
        {
            logQueue.Enqueue(item);
            return this;
        }

        public void Run()
        {
            new Thread(() =>
            {
                while (threadService.MainThread?.IsAlive != false)
                {
                    try
                    {
                        var sp = serviceProvider.CreateScope().ServiceProvider;
                        var loggerService = sp.GetService<ILoggerService>();
                        try
                        {
                            while (requests.Count > 0)
                            {
                                LogRequest request;
                                lock (requests)
                                {
                                    request = requests.Dequeue();
                                }
                                var type = request.NewObj.GetType();
                                var diff = new Dictionary<string, object>();
                                foreach (var property in type.GetProperties())
                                {
                                    if (!property.PropertyType.IsPrimitive()) continue;
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
                while (threadService.MainThread?.IsAlive != false)
                {
                    try
                    {
                        var sp = serviceProvider.CreateScope().ServiceProvider;
                        var loggerService = sp.GetService<ILoggerService>();
                        try
                        {
                            using var scope = serviceProvider.CreateScope();
                            lock (logQueue)
                            {
                                var dbContext = scope.ServiceProvider.GetService<LogDbContext>();
                                while (logQueue.Count > 0)
                                {
                                    var item = logQueue.Dequeue();
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
