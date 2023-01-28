using System.Collections.Generic;

namespace Devor.Framework.Utils.Queries
{
    public class FilterQueryOptionExpression
    {
        public virtual string ToSql(Dictionary<string, object> parameters)
        {
            return string.Empty;
        }
    }
}
