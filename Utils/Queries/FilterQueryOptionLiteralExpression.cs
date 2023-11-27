namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionLiteralExpression : FilterQueryOptionExpression
{
    public object Value { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        switch (Type.GetTypeCode(Value.GetType()))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return Value.ToString();
        }
        return Value is DateTime ? $"{Value}" : Value.ToString();
    }
}