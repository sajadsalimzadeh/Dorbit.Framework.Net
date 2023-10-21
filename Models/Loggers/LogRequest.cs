using Dorbit.Entities.Abstractions;
using Dorbit.Enums;
using Dorbit.Models.Abstractions;

namespace Dorbit.Models.Loggers
{
    public class LogRequest
    {
        public string Module { get; set; }
        public IEntity NewObj { get; set; }
        public LogAction Action { get; set; }
        public IEntity OldObj { get; set; }
        public IUserDto User { get; set; }
    }
}
