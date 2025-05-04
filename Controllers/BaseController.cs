using System;
using System.Security.Authentication;
using AutoMapper;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Controllers;

[Route("[controller]"), ApiController]
public abstract class BaseController : ControllerBase
{
    private IServiceProvider _serviceProvider;
    protected IServiceProvider ServiceProvider => _serviceProvider ??= HttpContext.RequestServices;

    private IUserResolver _userResolver;
    protected IUserResolver UserResolver => _userResolver ??= ServiceProvider.GetRequiredService<IUserResolver>();

    protected IMapper Mapper => ServiceProvider.GetService<IMapper>();

    protected T GetUserId<T>() => (UserResolver.User is not null ? (T)UserResolver.User.GetId() : throw new UnauthorizedAccessException());
    protected Guid GetUserId() => GetUserId<Guid>();
    protected QueryOptions QueryOptions => ODataQueryOptions.Parse(Request);

    protected CommandResult Succeed()
    {
        return new CommandResult(true);
    }

    protected CommandResult Failed(string message)
    {
        return new CommandResult(message);
    }
}

[Route("api/[controller]")]
public class BaseApiController : BaseController;