using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
            var logger = context.RequestServices.GetService<ILogger>();
            var op = new ExceptionResult<IDictionary>
            {
                Code = 500,
                Success = false,
                Message = ex.Message
            };

            logger?.Error(ex, ex.Message);
            if (context.Items.TryGetValue("UserId", out var userId))
            {
                var authenticationService = context.RequestServices.GetService<IIdentityService>();
                if (authenticationService is not null && await authenticationService.HasAccessAsync(userId?.ToString(), "Developer"))
                {
                    op.Data = ex.Data;
                    op.StackTrace = ex.StackTrace;
                }
            }
#if DEBUG
            op.StackTrace = ex.StackTrace;
#endif

            switch (ex)
            {
                case UnauthorizedAccessException unauthorizedAccessException:
                    op.Code = StatusCodes.Status403Forbidden;
                    op.Message = unauthorizedAccessException.Message.IsNullOrEmpty() ? Errors.UnAuthorize.ToString() : unauthorizedAccessException.Message;
                    break;
                case AuthenticationException authenticationException:
                    op.Code = StatusCodes.Status401Unauthorized;
                    op.Message = authenticationException.Message.IsNullOrEmpty() ? Errors.AuthenticationFailed.ToString() : authenticationException.Message;
                    break;
                case OperationException operationException:
                    op.Code = 400;
                    op.Data = operationException.Data;
                    op.Message = operationException.Message;
                    op.Messages = operationException.Messages;
                    if (logger is not null && operationException.ExceptionLog is not null)
                    {
                        logger.Write(operationException.ExceptionLog.Level, operationException.ExceptionLog.Message, operationException.ExceptionLog.Params);
                    }

                    break;
                case ModelValidationException modelValidationException:
                    op.Code = (int)HttpStatusCode.BadRequest;
                    op.Data = modelValidationException.Errors.ToDictionary(x => x.Field, x => x.Message);
                    op.Message = modelValidationException.Message;
                    break;
                default:
                    op.Code = (int)HttpStatusCode.InternalServerError;
                    op.Message ??= Errors.ServerError.ToString();
                    break;
            }

            context.Response.StatusCode = op.Code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(op, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}