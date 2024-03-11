using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts;

public class CommandResult
{
    public int Code { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; } = true;
    public IEnumerable<Enum> Messages { get; set; }

    public CommandResult()
    {
    }

    public CommandResult(bool success)
    {
        Success = success;
    }

    public CommandResult(string message)
    {
        Message = message;
    }

    public CommandResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static CommandResult Succeed()
    {
        return new CommandResult()
        {
            Success = true
        };
    }

    public static CommandResult Failed(object message)
    {
        return new CommandResult()
        {
            Success = false,
            Message = message.ToString()
        };
    }
}