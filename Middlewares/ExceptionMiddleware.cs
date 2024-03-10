using System.Net;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Models;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Dorbit.Framework.Middlewares;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            var authenticationService = context.RequestServices.GetService<IAuthService>();
            var logger = context.RequestServices.GetService<ILogger>();
            var op = new QueryResult<object>
            {
                Code = 500,
                Success = false
            };

            op.Message = ex.Message;
            logger?.Error(ex, ex.Message);
            if (context.Items.TryGetValue("UserId", out var userId) && userId is Guid userGuid)
            {
                if (await authenticationService.HasAccessAsync(userGuid, "Developer"))
                {
                    op.Data = ex.StackTrace;
                }
            }
            op.Data = ex.StackTrace;

            switch (ex)
            {
                case UnauthorizedAccessException unauthorizedAccessException:
                    if (unauthorizedAccessException.Message == "AccessDenied")
                    {
                        op.Code = StatusCodes.Status403Forbidden;
                        op.Message = Errors.AccessDenied.ToString();
                    }
                    else
                    {
                        op.Code = StatusCodes.Status401Unauthorized;
                        op.Message = Errors.UnAuthorized.ToString();
                    }

                    break;
                case OperationException operationException:
                    op.Code = (int)HttpStatusCode.BadRequest;
                    op.Data = operationException.Data;
                    op.Message = operationException.Message;
                    op.Messages = operationException.Messages;
                    break;
                case ModelValidationException modelValidationException:
                    op.Code = (int)HttpStatusCode.BadRequest;
                    op.Data = modelValidationException.Errors;
                    op.Message = modelValidationException.Message;
                    break;
                default:
                    op.Code = (int)HttpStatusCode.InternalServerError;
                    op.Message = Errors.ServerError.ToString();
                    break;
            }

            context.Response.StatusCode = op.Code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(op, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }
    }
}