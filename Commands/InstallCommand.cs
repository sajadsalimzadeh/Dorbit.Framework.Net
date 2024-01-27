using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class InstallCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    
    public override string Message { get; } = "Install";
    public override int Order { get; } = 100;


    public InstallCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task InvokeAsync(ICommandContext context)
    {
        var migrationCommandAll = _serviceProvider.GetService<MigrationCommandAll>();
        await migrationCommandAll.InvokeAsync(context);
        
        var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var entryFilename = Path.Combine(baseDir, assemblyName + ".exe");
        Process.Start("sc", $"create {assemblyName} binpath= \"{entryFilename} run\" start= auto");
        Process.Start("sc", $"start {assemblyName}");
    }
}