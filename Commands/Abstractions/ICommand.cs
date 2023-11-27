﻿using Dorbit.Framework.Models.Commands;

namespace Dorbit.Framework.Commands.Abstractions;

public interface ICommand
{
    bool IsRoot { get; }
    string Message { get; }
    IEnumerable<CommandParameter> GetParameters(ICommandContext context);
    IEnumerable<ICommand> GetSubCommands(ICommandContext context);
    Task Invoke(ICommandContext context);
}