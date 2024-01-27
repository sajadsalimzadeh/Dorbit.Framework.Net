using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Exceptions;
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
                    op.Code = StatusCodes.Status403Forbidden;
                    op.Message = Errors.AccessDenied.ToString();
                    break;
                case AuthenticationException authenticationException:
                    op.Code = StatusCodes.Status401Unauthorized;
                    op.Message = Errors.UnAuthorized.ToString();
                    break;
                case OperationException operationException:
                    op.Code = (int)HttpStatusCode.BadRequest;
                    op.Data = operationException.Data;
                    op.Message = operationException.Message;
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