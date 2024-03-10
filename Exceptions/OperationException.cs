using System.Net;

namespace Dorbit.Framework.Exceptions;

public class OperationException : Exception
{
    public int Code { get; set; }
    public dynamic Entities { get; }
    public IEnumerable<Enum> Messages { get; set; }

    public OperationException(string message) : base(message)
    {
        
    }
    
    public OperationException(params Enum[] messages) : base(messages.First().ToString())
    {
        Code = Convert.ToInt32(messages.First());
        Messages = messages;
    }

    public OperationException(dynamic data, params Enum[] messages) : this(messages)
    {
        Entities = data;
    }
    
    public OperationException(HttpStatusCode code, params Enum[] messages) : this(messages)
    {
        Code = (int)code;
    }

    public OperationException(HttpStatusCode code, dynamic data, params Enum[] messages) : this(messages)
    {
        Code = (int)code;
        Entities = data;
    }
}