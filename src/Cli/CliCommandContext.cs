using Devor.Framework.Services.Abstractions;
using System;
using System.Collections.Generic;

namespace Devor.Framework.Cli
{
    public class CliCommandContext : ICommandContext
    {
        public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }
    }
}
