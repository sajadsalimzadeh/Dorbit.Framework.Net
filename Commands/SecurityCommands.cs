using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Models.Commands;
using Microsoft.Extensions.DependencyInjection;
using Mobicar.Gateway.Commands;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class SecurityCommands : Command
{
    private readonly IServiceProvider _serviceProvider;
    public override string Message { get; } = "Security Commands";

    public SecurityCommands(IServiceProvider serviceProvider)
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

    public override Task Invoke(ICommandContext context)
    {
        return Task.CompletedTask;
    }
}