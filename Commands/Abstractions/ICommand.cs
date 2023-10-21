using Dorbit.Models.Commands;

namespace Dorbit.Commands.Abstractions;

public interface ICommand
{
    bool IsRoot { get; }
    string Message { get; }
    IEnumerable<CommandParameter> GetParameters(ICommandContext context);
    IEnumerable<ICommand> GetSubCommands(ICommandContext context);
    void Invoke(ICommandContext context);
}