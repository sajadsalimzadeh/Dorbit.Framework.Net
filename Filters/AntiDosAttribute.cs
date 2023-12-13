using System.Collections.Concurrent;
using System.Net;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

public class AntiDosAttribute : ActionFilterAttribute
{
    public int MaxRequestCountPerMinute { get; set; } = 200;

    private class RequestModel
    {
        public DateTime Time { get; set; }
    }

    private static readonly ConcurrentDictionary<long, List<RequestModel>> AllUserRequests = new();

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userResolver = context.HttpContext.RequestServices.GetService<IUserResolver>();
        var user = userResolver.User;

        long key = 0;
        var remoteAddress = context.HttpContext.Connection.RemoteIpAddress ?? new IPAddress(0);
        if (user is null )
        {
            if (long.TryParse(remoteAddress.ToString().Replace(".", "").Replace(":", ""), out var ipNumber))
            {
                key = 1_000_000_000_000 + ipNumber;
            }
        }
        else
        {
            key = user.Id.GetHashCode();
        }

        var now = DateTime.UtcNow;
        if (!AllUserRequests.TryGetValue(key, out var requests))
        {
            AllUserRequests.TryAdd(key, requests = new List<RequestModel>());
        }

        requests.Add(new RequestModel() { Time = now });
        AllUserRequests[key] = requests = requests.Where(x => x.Time.AddMinutes(1) > now).ToList();
        if (requests.Count > MaxRequestCountPerMinute) throw new OperationException(Errors.TooMuchRequest);
    }
}