using System.ComponentModel;

namespace Dorbit.Utils.Queries;

public enum FilterQueryOptionUnaryOperators
{
    [Description("None")]
    None = 0,
    [Description("Not"), FilterQueryOptionSql("NOT")]
    Not = 1,
}