using Devor.Framework.Database.Abstractions;
using Devor.Framework.Models.Abstractions;
using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Enums;

namespace Devor.Framework.Models.Loggers
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
