using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Contracts.Loggers;

public class LogEntityRequest
{
    public string Module { get; set; }
    public object OldObj { get; set; }
    public object NewObj { get; set; }
    public LogAction Action { get; set; }
    public IUserDto User { get; set; }
}