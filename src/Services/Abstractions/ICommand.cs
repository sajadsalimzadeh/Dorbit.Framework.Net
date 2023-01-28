using System.Collections.Generic;

namespace Devor.Framework.Services.Abstractions
{
    public interface ICommand
    {
        string Name { get; }
        IEnumerable<string> GetParameters();
        IEnumerable<ICommand> GetSubCommands();
        void Invoke(ICommandContext context);
    }
}
