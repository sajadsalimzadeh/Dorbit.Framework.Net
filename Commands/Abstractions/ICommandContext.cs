using System.Collections.Generic;

namespace Dorbit.Framework.Commands.Abstractions;

public interface ICommandContext
{
    Dictionary<string, object> Arguments { get; set; }

    void Error(string message);
    void Success(string message);
    void Log(string message);

    object GetArg(string name);
    string GetArgAsString(string name);
    int GetArgAsInt(string name);
}