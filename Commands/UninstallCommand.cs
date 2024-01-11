using System.Diagnostics;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class UninstallCommand : Command
{
    public override string Message { get; } = "Uninstall";
    public override int Order { get; } = 101;

    public override Task Invoke(ICommandContext context)
    {
        var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
        Process.Start("sc", $"stop {assemblyName}");
        Process.Start("sc", $"delete {assemblyName}");
        
        return Task.CompletedTask;
    }
}