using System.ComponentModel;

namespace Dorbit.Utils.Queries;

public enum FilterQueryOptionLogicalOperators
{
    [Description("None")]
    None = 0,
    [Description("And"), FilterQueryOptionSql("AND")]
    And = 1,
    [Description("Or"), FilterQueryOptionSql("OR")]
    Or = 2,
}