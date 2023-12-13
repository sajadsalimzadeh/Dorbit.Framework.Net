namespace Dorbit.Framework.Exceptions;

public class OperationException : Exception
{
    public int Code { get; set; }

    public OperationException(Enum e) : base(e.ToString())
    {
        Code = Convert.ToInt32(e);
    }
    public OperationException(string message) : base(message)
    {
    }
}