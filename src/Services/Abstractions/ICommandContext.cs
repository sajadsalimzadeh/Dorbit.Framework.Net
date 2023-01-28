using System.Collections.Generic;

namespace Devor.Framework.Services.Abstractions
{
    public interface ICommandContext
    {
        Dictionary<string, string> Arguments { get; set; }

        void Error(string message);
        void Log(string message);
    }
}
