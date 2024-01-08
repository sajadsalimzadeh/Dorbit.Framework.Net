using System.ComponentModel;

namespace Dorbit.Framework.Enums;

public enum LogAction : byte
{
    [Description("Unknown")]
    None = 0,
    [Description("Insert")]
    Insert = 1,
    [Description("Update")]
    Update = 2,
    [Description("Delete")]
    Delete = 3,
}