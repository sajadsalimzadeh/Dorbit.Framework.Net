using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Middlewares;

[ServiceSingletone]
public class AuthMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var identityRequest = context.GetIdentityRequest();

        if (identityRequest.AccessToken.IsNotNullOrEmpty())
        {
            var serviceProvider = context.RequestServices;
            var identityService = serviceProvider.GetService<IIdentityService>();
            await identityService.ValidateAsync(identityRequest);
        }
        
        await next(context);
    }
}