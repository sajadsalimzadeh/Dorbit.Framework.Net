using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Contracts.Loggers;

public class LogRequest
{
    public string Module { get; set; }
    public object OldObj { get; set; }
    public object NewObj { get; set; }
    public LogAction Action { get; set; }
    public IUserDto User { get; set; }
}