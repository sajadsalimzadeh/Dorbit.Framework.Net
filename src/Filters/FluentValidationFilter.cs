using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Devor.Framework.Filters
{
    public class FluentValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }
    }
}
