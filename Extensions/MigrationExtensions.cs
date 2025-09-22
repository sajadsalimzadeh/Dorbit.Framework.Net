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
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.MigrateAll();
    }

    public static async Task MigrateAll(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger>();
        var dbContextMigrators = serviceProvider.GetServices<IDbContextMigrator>();
        foreach (var migrator in dbContextMigrators)
        {
            try
            {
                await migrator.MigrateAsync();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Migration Exception");
                throw;
            }
        }
    }
}