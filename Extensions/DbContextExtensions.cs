using Dorbit.Databases.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Extensions;

public static class DbContextExtensions
{
    public static void RunMigrations(this WebApplication app)
    {
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        var dbContexts = serviceProvider.GetServices<IDbContext>();
        foreach (var dbContext in dbContexts)
        {
            dbContext.Migrate();
        }
    }
}