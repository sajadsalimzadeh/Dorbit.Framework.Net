using System.ComponentModel;

namespace Dorbit.Framework.Utils.Queries;

public enum FilterQueryOptionBinaryOperators
{
    [Description("None")]
    None = 0,
    [Description("Equal"), FilterQueryOptionSql("=")]
    Eq = 1,
    [Description("Not Equal"), FilterQueryOptionSql("!=")]
    Ne = 2,
    [Description("Greater Than"), FilterQueryOptionSql(">")]
    Gt = 3,
    [Description("Greater And Equal"), FilterQueryOptionSql(">=")]
    Ge = 4,
    [Description("Less Than"), FilterQueryOptionSql("<")]
    Lt = 5,
    [Description("Less And Equal"), FilterQueryOptionSql("<=")]
    Le = 6,
    [Description("Add"), FilterQueryOptionSql("+")]
    Add = 7,
    [Description("Subtract"), FilterQueryOptionSql("-")]
    Sub = 8,
    [Description("Multiplex"), FilterQueryOptionSql("*")]
    Mul = 9,
    [Description("Division"), FilterQueryOptionSql("/")]
    Div = 10,
    [Description("Mod"), FilterQueryOptionSql("%")]
    Mod = 11,
    [Description("Like"), FilterQueryOptionSql("LIKE"), FilterQueryOptionFormat("{0}.Contains({2})")]
    Like = 12,
    [Description("Not Like"), FilterQueryOptionSql("NOT LIKE"), FilterQueryOptionFormat("!{0}.Contains({2})")]
    NotLike = 13,
}