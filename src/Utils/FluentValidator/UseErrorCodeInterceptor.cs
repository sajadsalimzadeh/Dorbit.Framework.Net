using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Devor.Framework.Attributes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

namespace Devor.Framework.Utils.FluentValidator
{
    [ServiceRegisterar]
    public class UseErrorCodeInterceptor : IValidatorInterceptor
    {
        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext validationContext)
        {
            return validationContext;
        }

        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            return result;
        }
    }
}
