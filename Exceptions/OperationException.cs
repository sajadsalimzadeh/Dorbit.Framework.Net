using System;
using System.Collections.Generic;
using System.Net;
using Serilog.Events;

namespace Dorbit.Framework.Exceptions;

public class OperationException : Exception
{
    public int Code { get; init; }
    public dynamic Entities { get; init; }
    public IEnumerable<Enum> Messages { get; init; }
    public ExceptionLogDto ExceptionLog { get; init; }
    
    public OperationException(string message) : base(message)
    {
    }

    public OperationException(Enum message, ExceptionLogDto exceptionLog = null) : base(message.ToString())
    {
        Code = Convert.ToInt32(message);
        Messages = [message];
        ExceptionLog = exceptionLog;
    }
    
    public OperationException(HttpStatusCode code, Enum message, ExceptionLogDto exceptionLog = null) : this(message, exceptionLog)
    {
        Code = (int)code;
    }
}

public class ExceptionLogDto
{
    public LogEventLevel Level { get; set; } = LogEventLevel.Error;
    public string Message { get; set; }
    public object[] Params { get; set; }


    public ExceptionLogDto(string message, params object[] @params)
    {
        Message = message;
        Params = @params;
    }
}