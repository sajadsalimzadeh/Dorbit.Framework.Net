using Devor.Framework.Services.Abstractions;
using System.Collections.Generic;

namespace Devor.Framework.Services
{
    public abstract class Command : ICommand
    {
        public virtual string Name => GetType().Name.Replace("Command", "");

        public virtual IEnumerable<string> GetParameters()
        {
            return new List<string>();
        }

        public virtual IEnumerable<ICommand> GetSubCommands()
        {
            return new List<ICommand>();
        }

        public abstract void Invoke(ICommandContext context);
    }
}
