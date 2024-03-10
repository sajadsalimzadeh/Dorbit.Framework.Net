using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Contracts.Commands;

namespace Dorbit.Framework.Commands;

public abstract class Command : ICommand
{
    public abstract string Message { get; }
    public virtual bool IsRoot => true;
    public virtual int Order { get; } = 0;

    public virtual IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        return new List<CommandParameter>();
    }

    public virtual IEnumerable<ICommand> GetSubCommands(ICommandContext context)
    {
        return new List<ICommand>();
    }

    public abstract Task InvokeAsync(ICommandContext context);
}