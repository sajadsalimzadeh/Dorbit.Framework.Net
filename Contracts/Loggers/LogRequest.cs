using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Enums;
using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Models.Loggers;

public class LogRequest
{
    public string Module { get; set; }
    public IEntity NewObj { get; set; }
    public LogAction Action { get; set; }
    public IEntity OldObj { get; set; }
    public IUserDto User { get; set; }
}