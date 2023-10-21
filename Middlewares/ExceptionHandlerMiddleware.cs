using System.Net;
using Dorbit.Exceptions;
using Dorbit.Models;
using Dorbit.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dorbit.Middlewares
{
    public static class ExceptionHandlerMiddleware
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var authenticationService = context.RequestServices.GetService<IAuthenticationService>();
                    var logger = context.RequestServices.GetService<ILoggerService>();
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    var op = new QueryResult<object>
                    {
                        Code = 500,
                        Success = false
                    };
                    if (exception is not null)
                    {
                        op.Message = exception.Message;
                        logger.LogError(exception);
                        if (authenticationService.HasPolicy("Developer"))
                        {
                            op.Data = exception.StackTrace;
                        }
                        switch (exception)
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
                    }
                    context.Response.StatusCode = op.Code;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(op, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }));
                });
            });
            return app;
        }
    }
}
