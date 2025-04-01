using System.Collections.Generic;
using System.Linq;

namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionExpression
{
    public virtual string ToSql(Dictionary<string, object> parameters)
    {
        return string.Empty;
    }
}