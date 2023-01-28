using System.ComponentModel;

namespace Devor.Framework.Utils.Queries
{
    public enum FilterQueryOptionUnaryOperators
    {
        [Description("None")]
        None = 0,
        [Description("Not"), FilterQueryOptionSql("NOT")]
        Not = 1,
    }
}
