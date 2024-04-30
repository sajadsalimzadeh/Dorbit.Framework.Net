using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Commands;

namespace Dorbit.Framework.Commands.Abstractions;

public interface ICommand
{
    bool IsRoot { get; }
    string Message { get; }
    int Order { get; }
    IEnumerable<CommandParameter> GetParameters(ICommandContext context);
    IEnumerable<ICommand> GetSubCommands(ICommandContext context);
    Task InvokeAsync(ICommandContext context);
    Task AfterEnterAsync(ICommandContext context);
}