using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using Dorbit.Attributes;
using Dorbit.Models;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Middlewares;

[ServiceRegister]
public class GlobalErrorHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var result = new CommandResult()
            {
                Success = false,
                Message = error?.Message,
            };
            switch (error)
            {
                case AuthenticationException e:
                    result.Message = "UnAuthorized";
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case UnauthorizedAccessException e:
                    result.Message = "AccessDenied";
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case KeyNotFoundException e:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case ValidationException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            result.Code = response.StatusCode;
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}