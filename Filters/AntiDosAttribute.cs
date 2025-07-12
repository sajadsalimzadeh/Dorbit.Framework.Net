using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

public class AntiDosAttribute(AntiDosAttribute.DurationType type, int count) : ActionFilterAttribute
{
    public enum DurationType
    {
        Day = 1,
        Hour = 2,
        Minute = 3,
    }

    private TimeSpan _time = type switch
    {
        DurationType.Day => TimeSpan.FromDays(1),
        DurationType.Hour => TimeSpan.FromHours(1),
        DurationType.Minute => TimeSpan.FromMinutes(1),
        _ => TimeSpan.FromMinutes(1),
    };
    public int Count { get; set; } = count;

    private class RequestModel
    {
        public DateTime Time { get; set; }
    }

    private static readonly ConcurrentDictionary<string, List<RequestModel>> AllUserRequests = new();

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var remoteAddress = context.HttpContext.Connection.RemoteIpAddress ?? new IPAddress(0);
        var userResolver = context.HttpContext.RequestServices.GetService<IUserResolver>();
        var user = userResolver?.User;

        var key = user is null ? remoteAddress.ToString() : user.GetId().ToString() ?? string.Empty;

        var now = DateTime.UtcNow;
        var requests = AllUserRequests.GetOrAdd(key, []);
        requests.Add(new RequestModel { Time = now });
        requests.RemoveAll(x => x.Time.Add(_time) < now);
        if (requests.Count > Count) throw new OperationException(FrameworkErrors.TooMuchRequest);
    }
}