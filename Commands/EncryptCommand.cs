using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Contracts.Commands;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class EncryptCommand : Command
{
    public override bool IsRoot { get; } = false;
    public override string Message => "Encrypt String";

    public override IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        yield return new CommandParameter("Input", "Input");
        yield return new CommandParameter("Key", "Key");
    }

    public override Task InvokeAsync(ICommandContext context)
    {
        var protectedProperty = context.GetArgAsString("Input").GetEncryptedValue(context.GetArgAsString("Key").ToBytesUtf8());
        context.Log($"{protectedProperty.Value}\n");
        return Task.CompletedTask;
    }
}