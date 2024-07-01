using System;
using System.Threading.Tasks;
using Dorbit.Framework.Database.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Framework.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAll(this WebApplication app)
    {
        var sp = app.Services.CreateScope().ServiceProvider;
        await sp.MigrateAll();
    }

    public static async Task MigrateAll(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger>();
        var dbContexts = serviceProvider.GetServices<IDbContext>();
        foreach (var dbContext in dbContexts)
        {
            try
            {
                await dbContext.MigrateAsync();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Migration Exception");
                throw;
            }
        }
    }
}