using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Commands;

[ServiceRegister(Order = 100)]
public class InstallCommand : Command
{
    private readonly IServiceProvider _serviceProvider;

    public override string Message { get; } = "Install or Update";
    public override int Order { get; } = 100;


    public InstallCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task InvokeAsync(ICommandContext context)
    {
        var migrationCommandAll = _serviceProvider.GetService<MigrationCommandAll>();
        await migrationCommandAll.InvokeAsync(context);
        
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        var entryFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".exe");

        var createProcess = Process.Start("cmd", $"/C sc create {assemblyName} binpath= \"{entryFilename} run\" start= auto");
        context.Log($"\n==========Creating Service==========\n");
        await createProcess?.WaitForExitAsync()!;

        var startProcess = Process.Start("cmd", $"/C sc start {assemblyName}");
        context.Log($"\n==========Starting Service==========\n");
        await startProcess?.WaitForExitAsync()!;
    }
}