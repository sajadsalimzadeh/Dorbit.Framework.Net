using System.Threading.Tasks;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Extensions;

public static class CommandExtensions
{
    public static async Task RunCliAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var cliRunnerService = serviceProvider.GetService<CliRunnerService>();
        try
        {
            await cliRunnerService.RunAsync(app);
        }
        catch
        {
            await app.RunAsync();
        }
    }
}