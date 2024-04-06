using System;
using System.Threading.Tasks;
using Dorbit.Framework.Database.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAll(this WebApplication app)
    {
        var sp = app.Services.CreateScope().ServiceProvider;
        await MigrateAll(sp);
    }
    
    public static async Task MigrateAll(this IServiceProvider serviceProvider)
    {
        var dbContexts = serviceProvider.GetServices<IDbContext>();
        foreach (var dbContext in dbContexts)
        {
            await dbContext.MigrateAsync();
        }
    }
}