using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class SecurityModuleCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    public override string Message { get; } = "Security Module";

    public SecurityModuleCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override IEnumerable<ICommand> GetSubCommands(ICommandContext context)
    {
        yield return _serviceProvider.GetService<CreateTokenCommand>();
        yield return _serviceProvider.GetService<DecryptCommand>();
        yield return _serviceProvider.GetService<EncryptCommand>();
        yield return _serviceProvider.GetService<ValidateTokenCommand>();
    }

    public override Task InvokeAsync(ICommandContext context)
    {
        return Task.CompletedTask;
    }
}