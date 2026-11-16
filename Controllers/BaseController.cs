using System;
using AutoMapper;
using Dorbit.Framework.Contracts.Identities;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Framework.Controllers;

[Route("[controller]"), ApiController]
public abstract class BaseController : ControllerBase
{
    protected IServiceProvider ServiceProvider => field ??= HttpContext.RequestServices;
    protected IUserResolver UserResolver => field ??= ServiceProvider.GetRequiredService<IUserResolver>();
    protected IIdentityService IdentityService => field ??= ServiceProvider.GetRequiredService<IIdentityService>();
    protected IdentityDto Identity => IdentityService.Identity;
    protected ILogger Logger => field ??= ServiceProvider.GetRequiredService<ILogger>();
    protected IMemoryCache MemoryCache => field ??= ServiceProvider.GetRequiredService<IMemoryCache>();
    protected IMapper Mapper => field ??= ServiceProvider.GetService<IMapper>();
    protected QueryOptions QueryOptions => field ??= ODataQueryOptions.Parse(Request);

    protected Guid GetUserId() => UserResolver.User?.GetId() ?? throw new UnauthorizedAccessException();

    protected CommandResult Succeed()
    {
        return new CommandResult(true);
    }

    protected CommandResult Failed(string message)
    {
        return new CommandResult(message);
    }
}