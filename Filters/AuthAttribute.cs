using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthAttribute(params string[] accesses) : Attribute, IAsyncActionFilter
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();
    
    public string[] Accesses { get; set; } = accesses;
    public bool IsOptional { get; set; }
    public int MaxConcurrencyRequest { get; set; }
    public UserConcurrencyLevel ConcurrencyLevel { get; set; }

    public enum UserConcurrencyLevel
    {
        Application = 0,
        Controller = 1,
        Action = 2
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var accesses = Accesses;
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            throw new Exception("Context is not type of ControllerActionDescriptor");

        if (accesses.IsNotNullOrEmpty())
        {
            var type = actionDescriptor.ControllerTypeInfo as Type;
            var preType = type;
            while (type is not null)
            {
                if (type.IsAssignableFrom(typeof(CrudController)))
                {
                    var index = 0;
                    foreach (var genericType in preType.GenericTypeArguments)
                    {
                        var name = genericType.GetCustomAttribute<AuthTypeNameAttribute>()?.Name ?? genericType.Name;
                        accesses = accesses.Select(x => x.Replace("{type" + index + "}", name)).ToArray();
                        index++;
                    }

                    break;
                }

                preType = type;
                type = type.BaseType;
            }
        }

        var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
        var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
        var methodHasAuthAttribute = controllerAuthAttributes.Any(x => Equals(x, this)) && methodAttributes.Any();
        var hasIgnoreAuthIgnoreAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null;
        if (methodHasAuthAttribute || hasIgnoreAuthIgnoreAttribute)
        {
            accesses = [];
        }

        var authPrefixAttribute = actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AuthPrefixAttribute>();
        if (authPrefixAttribute is not null)
        {
            accesses = accesses.Select(x => authPrefixAttribute.Prefix + "-" + x).ToArray();
        }

        var userId = Guid.NewGuid();
        try
        {
            var identityRequest = context.HttpContext.GetIdentityRequest();
            var serviceProvider = context.HttpContext.RequestServices;
            
            var identityService = serviceProvider.GetService<IIdentityService>();
            var identity = identityService.Identity ?? await identityService.ValidateAsync(identityRequest);
            
            if (identity is null)
                throw new AuthenticationException();

            userId = identity.User.GetId();
            
            if (accesses is not null && accesses.Length > 0 && !identity.IsFullAccess && !accesses.Any(identity.HasAccess))
                throw new UnauthorizedAccessException();

            foreach (var parameter in context.ActionDescriptor.Parameters)
            {
                var fromClaimAttribute = parameter.ParameterType.GetCustomAttribute<FromClaimAttribute>();
                if (fromClaimAttribute is null) continue;

                var claimDto = identity.Claims.FirstOrDefault(x => x.Type.Equals(fromClaimAttribute.Type, StringComparison.CurrentCultureIgnoreCase));
                if (claimDto == null || claimDto.Value.IsNullOrEmpty())
                    throw new UnauthorizedAccessException($"claim {fromClaimAttribute.Type} not found");

                if (parameter.ParameterType == typeof(Guid))
                {
                    context.ActionArguments[parameter.Name] = Guid.Parse(claimDto.Value);
                }
                else if (parameter.ParameterType == typeof(int))
                {
                    context.ActionArguments[parameter.Name] = int.Parse(claimDto.Value);
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    context.ActionArguments[parameter.Name] = claimDto.Value;
                }
                else
                {
                    context.ActionArguments[parameter.Name] = JsonSerializer.Deserialize(claimDto.Value, parameter.ParameterType);
                }
            }
        }
        catch
        {
            if (!IsOptional)
            {
                throw;
            }
        }

        if (MaxConcurrencyRequest > 0)
        {
            var key = ConcurrencyLevel switch
            {
                UserConcurrencyLevel.Application => $"{userId}",
                UserConcurrencyLevel.Controller => $"{actionDescriptor.ControllerName}-{userId}",
                UserConcurrencyLevel.Action => $"{actionDescriptor.ControllerName}-{actionDescriptor.ActionName}-{userId}",
                _ => throw new ArgumentOutOfRangeException()
            };
            var semaphore = _userLocks.GetOrAdd(key, new SemaphoreSlim(MaxConcurrencyRequest, MaxConcurrencyRequest));
            try
            {
                await semaphore.WaitAsync();
                await next.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }
        else
        {
            await next.Invoke();
        }
    }
}