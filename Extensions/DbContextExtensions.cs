using Dorbit.Framework.Database.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Extensions;

public static class DbContextExtensions
{
    public static async Task RunMigrations(this WebApplication app)
    {
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        var dbContexts = serviceProvider.GetServices<IDbContext>();
        foreach (var dbContext in dbContexts)
        {
            await dbContext.MigrateAsync();
        }
    }
}