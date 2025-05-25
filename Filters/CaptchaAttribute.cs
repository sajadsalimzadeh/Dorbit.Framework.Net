using System;
using System.Threading.Tasks;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CaptchaAttribute : Attribute, IAsyncActionFilter
{

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var captchaService = context.HttpContext.RequestServices.GetService<CaptchaService>();
        if (!context.HttpContext.Request.Headers.TryGetValue("Captcha", out var captcha))
            throw new OperationException(Errors.CaptchaNotSet);

        var captchaKeyValue = captcha.ToString().Split(' ');
        if (captchaKeyValue.Length != 2)
            throw new OperationException(Errors.CaptchaNotCorrect);

        if(!captchaService.Validate(captchaKeyValue[0], captchaKeyValue[1])) 
            throw new OperationException(Errors.CaptchaNotCorrect);
        
        return next();
    }
}