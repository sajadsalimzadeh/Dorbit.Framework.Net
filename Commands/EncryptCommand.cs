using Dorbit.Attributes;
using Dorbit.Commands.Abstractions;
using Dorbit.Models.Commands;
using Dorbit.Utils.Cryptography;

namespace Dorbit.Commands;

[ServiceRegister]
public class EncryptCommand : Command
{
    public override string Message => "Encrypt String";

    public override IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        yield return new CommandParameter("Input", "Input");
        yield return new CommandParameter("Key", "Key");
    }

    public override void Invoke(ICommandContext context)
    {
        var cypherText = Aes.Encrypt(context.Arguments["Input"].ToString(), context.Arguments["Key"].ToString());
        context.Log($"{cypherText}\n");
    }
}