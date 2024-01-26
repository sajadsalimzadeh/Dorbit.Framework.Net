using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Contracts.Loggers;

public class LogRequest
{
    public string Module { get; set; }
    public IEntity NewObj { get; set; }
    public LogAction Action { get; set; }
    public IEntity OldObj { get; set; }
    public IUserDto User { get; set; }
}