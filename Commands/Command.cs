using Dorbit.Commands.Abstractions;
using Dorbit.Models.Commands;

namespace Dorbit.Commands;

public abstract class Command : ICommand
{
    public abstract string Message { get; }
    public virtual bool IsRoot => true;

    public virtual IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        return new List<CommandParameter>();
    }

    public virtual IEnumerable<ICommand> GetSubCommands(ICommandContext context)
    {
        return new List<ICommand>();
    }

    public abstract void Invoke(ICommandContext context);
}