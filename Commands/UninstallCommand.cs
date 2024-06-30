using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;

namespace Dorbit.Framework.Commands;

[ServiceRegister(Order = 101)]
public class UninstallCommand : Command
{
    public override string Message { get; } = "Uninstall";
    public override int Order { get; } = 101;

    public override async Task InvokeAsync(ICommandContext context)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        var stopProcess = Process.Start(new ProcessStartInfo("sc", $"stop {assemblyName}")
        {
            Verb = "runas",
            RedirectStandardInput = false,
        });
        await stopProcess?.WaitForExitAsync()!;

        var deleteProcess = Process.Start(new ProcessStartInfo("sc", $"delete {assemblyName}")
        {
            Verb = "runas",
            RedirectStandardInput = false,
        });
        await deleteProcess?.WaitForExitAsync()!;
    }
}