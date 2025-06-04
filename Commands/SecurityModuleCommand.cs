using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class SecurityModuleCommand(IServiceProvider serviceProvider) : Command
{
    public override string Message { get; } = "Security Module";

    public override IEnumerable<ICommand> GetSubCommands(ICommandContext context)
    {
        yield return serviceProvider.GetService<DecryptCommand>();
        yield return serviceProvider.GetService<EncryptCommand>();
    }

    public override Task InvokeAsync(ICommandContext context)
    {
        return Task.CompletedTask;
    }
}