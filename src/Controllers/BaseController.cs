using Devor.Framework.Models;
using Devor.Framework.Utils.Queries;
using Devor.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using Devor.Framework.Filters;
using AutoMapper;

namespace Devor.Framework.Controllers
{
    [Monitor]
    [Route("[controller]"), ApiController]
    public abstract class BaseController : ControllerBase
    {
        private IServiceProvider serviceProvider;
        protected IServiceProvider ServiceProvider => serviceProvider ??= HttpContext.RequestServices;

        protected IUserResolver userResolver;
        protected IUserResolver UserResolver => userResolver ??= ServiceProvider.GetRequiredService<IUserResolver>();

        protected IMapper Mapper => ServiceProvider.GetService<IMapper>();

        protected long UserId => UserResolver.GetUser().Id;
        protected QueryOptions QueryOptions => new ODataQueryOptions().Parse(Request);

        protected OperationResult Succeed()
        {
            return new OperationResult(true);
        }

        protected OperationResult Failed(string message)
        {
            return new OperationResult(message);
        }
    }
}
