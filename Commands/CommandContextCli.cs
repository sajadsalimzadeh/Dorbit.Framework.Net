using System;

namespace Dorbit.Framework.Commands;

public class CommandContextCli : CommandContextBase
{
    public override void Log(string message)
    {
        Log(message, ConsoleColor.White);
    }

    public override void Error(string message)
    {
        Log(message, ConsoleColor.Red);
    }

    public override void Success(string message)
    {
        Log(message, ConsoleColor.Green);
    }

    private void Log(string message, ConsoleColor color)
    {
        var preColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = preColor;
    }
}