using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Extensions;

public static class CommandExtensions
{
    public static void RunCli(this WebApplication app)
    {
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        var cliRunnerService = serviceProvider.GetService<CliRunnerService>();
        cliRunnerService.Run(app);
    }
}