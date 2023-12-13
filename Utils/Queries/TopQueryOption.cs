namespace Dorbit.Framework.Utils.Queries;

public class TopQueryOption
{
    public int Value { get; set; }

    public TopQueryOption Clone()
    {
        return new TopQueryOption()
        {
            Value = Value
        };
    }
}