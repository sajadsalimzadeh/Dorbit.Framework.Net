using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Models.Commands;
using Dorbit.Framework.Services;

namespace Mobicar.Gateway.Commands;

[ServiceRegister]
public class ValidateTokenCommand : Command
{
    private readonly JwtService _jwtService;
 
    public override bool IsRoot { get; } = false;
    public override string Message => "Validate Token";

    public ValidateTokenCommand(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public override IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        yield return new CommandParameter("Key", "Key");
    }
    
    public override async Task Invoke(ICommandContext context)
    {
        var result = await _jwtService.TryValidateTokenAsync(context.GetArgAsString("Key"));
        if(result) context.Success("Token Is Valid");
        else context.Error("Token Is InValid");
    }
}