using Dorbit.Exceptions;
using Dorbit.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Filters
{
    public class AntiDosAttribute : ActionFilterAttribute
    {
        public int MaxRequestCountPerMinute { get; set; } = 200;

        class RequestModel
        {
            public DateTime Time { get; set; }
        }

        static readonly Dictionary<long, List<RequestModel>> allUserRequests = new();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userResolver = context.HttpContext.RequestServices.GetService<IUserResolver>();
            var key = userResolver.GetUser()?.Id;

            var remoteAddress = context.HttpContext.Connection.RemoteIpAddress;
            if (!key.HasValue && long.TryParse(remoteAddress.ToString().Replace(".", "").Replace(":", ""), out var ipNumber))
            {
                key = 1_000_000_000_000 + ipNumber;
            }
            if (key.HasValue)
            {
                var keyValue = key.Value;
                List<RequestModel> requests;
                lock (allUserRequests)
                {
                    var now = DateTime.Now;
                    requests = (allUserRequests.ContainsKey(keyValue) ? allUserRequests[keyValue] : (allUserRequests[keyValue] = new List<RequestModel>()));
                    requests.Add(new RequestModel() { Time = now });
                    allUserRequests[keyValue] = requests = requests.Where(x => x.Time.AddMinutes(1) > now).ToList();
                }
                if (requests.Count > MaxRequestCountPerMinute) throw new OperationException(Errors.TooMuchRequest);
            }
        }
    }
}
