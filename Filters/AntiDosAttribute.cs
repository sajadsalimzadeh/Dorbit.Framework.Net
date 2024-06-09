using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Cms;

namespace Dorbit.Framework.Filters;

public class AntiDosAttribute : ActionFilterAttribute
{
    public enum DurationType
    {
        Day = 1,
        Hour = 2,
        Minute = 3,
    }

    private TimeSpan _time;
    public int Count { get; set; }

    public AntiDosAttribute(DurationType type, int count)
    {
        Count = count;
        _time = type switch
        {
            DurationType.Day => TimeSpan.FromDays(1),
            DurationType.Hour => TimeSpan.FromHours(1),
            DurationType.Minute => TimeSpan.FromMinutes(1),
            _ => TimeSpan.FromMinutes(1),
        };
    }

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

        var key = user is null ? remoteAddress.ToString() : user.Id.ToString() ?? string.Empty;

        var now = DateTime.UtcNow;
        var requests = AllUserRequests.GetOrAdd(key, []);
        requests.Add(new RequestModel { Time = now });
        var timespan = 
        AllUserRequests[key] = requests = requests.Where(x => x.Time.Add(_time) > now).ToList();
        if (requests.Count > Count) throw new OperationException(Errors.TooMuchRequest);
    }
}