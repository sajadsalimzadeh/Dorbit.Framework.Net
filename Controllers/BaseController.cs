using AutoMapper;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Models;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Controllers;

[Monitor]
[Route("[controller]"), ApiController]
public abstract class BaseController : ControllerBase
{
    private IServiceProvider serviceProvider;
    protected IServiceProvider ServiceProvider => serviceProvider ??= HttpContext.RequestServices;

    private IUserResolver userResolver;
    protected IUserResolver UserResolver => userResolver ??= ServiceProvider.GetRequiredService<IUserResolver>();

    protected IMapper Mapper => ServiceProvider.GetService<IMapper>();

    protected Guid? UserId => UserResolver.User?.Id;
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