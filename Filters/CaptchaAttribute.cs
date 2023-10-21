using Dorbit.Exceptions;
using Dorbit.Services;
using GoogleReCaptcha.V3.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Filters
{
    public class CaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if(!context.HttpContext.Request.Headers.ContainsKey("Captcha")) throw new OperationException(Errors.CaptchaNotSet);
            var captcha = context.HttpContext.Request.Headers["Captcha"].FirstOrDefault();
            if(string.IsNullOrEmpty(captcha)) throw new OperationException(Errors.CaptchaNotSet);
            var captchaSplit = captcha.Split(' ');
            if (captchaSplit.Length < 2)
            {
                var captchaValidator = context.HttpContext.RequestServices.GetService<ICaptchaValidator>();
                if (!captchaValidator.IsCaptchaPassedAsync(captcha).Result)
                {
                    throw new OperationException(Errors.CaptchaNotCorrect);
                }
            }
            else
            {
                var captchaService = context.HttpContext.RequestServices.GetService<CaptchaService>();
                var captchaKey = captchaSplit[0];
                var captchaValue = captchaSplit[1];
                if (!captchaService.Verify(captchaKey, captchaValue))
                {
                    throw new OperationException(Errors.CaptchaNotCorrect);
                }
            }
        }
    }
}
