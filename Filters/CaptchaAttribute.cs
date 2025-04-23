using Microsoft.AspNetCore.Mvc.Filters;

namespace Dorbit.Framework.Filters;

public class CaptchaAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        
    }
}