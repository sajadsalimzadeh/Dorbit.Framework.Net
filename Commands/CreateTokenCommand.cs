using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Contracts.Commands;
using Dorbit.Framework.Contracts.Jwts;
using Dorbit.Framework.Services;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class CreateTokenCommand(JwtService jwtService) : Command
{
    public override bool IsRoot { get; } = false;
    public override string Message => "Create Token";

    public override IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        yield return new CommandParameter("Id");
        yield return new CommandParameter("Name");
        yield return new CommandParameter("Accesses", "Enter Access (separate with comma) (default:admin)");
        yield return new CommandParameter("Lifetime", "Enter Lifetime (10s, 30m, 2h, 7d, 2w, 3M, 1y) (default:1h)");
    }

    public override async Task InvokeAsync(ICommandContext context)
    {
        var accesses = (context.GetArgAsString("Accesses") ?? "admin").Split(',');
        var lifetime = context.GetArgAsString("Lifetime") ?? "1h";
        var lifetimeValue = Convert.ToInt32(lifetime.Substring(0, lifetime.Length - 1));

        var expires = DateTime.UtcNow;
        if (lifetime.EndsWith("s")) expires = expires.AddSeconds(lifetimeValue);
        else if (lifetime.EndsWith("m")) expires = expires.AddMinutes(lifetimeValue);
        else if (lifetime.EndsWith("h")) expires = expires.AddHours(lifetimeValue);
        else if (lifetime.EndsWith("d")) expires = expires.AddDays(lifetimeValue);
        else if (lifetime.EndsWith("w")) expires = expires.AddDays(lifetimeValue * 7);
        else if (lifetime.EndsWith("M")) expires = expires.AddMonths(lifetimeValue);
        else if (lifetime.EndsWith("y")) expires = expires.AddYears(lifetimeValue);

        var request = new JwtCreateTokenRequest()
        {
            Expires = expires,
            Claims = accesses.ToDictionary(x => "access", x => x)
        };
        request.Claims.Add("Id", context.GetArgAsString("Id"));
        request.Claims.Add("Name", context.GetArgAsString("Name"));
        var createTokenResponse = await jwtService.CreateTokenAsync(request);
        context.Log($"Token: {createTokenResponse.Key}\n");
    }
}