using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Database.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Extensions;

public static class DbContextExtensions
{
    public static async Task RunMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContexts = serviceProvider.GetServices<IDbContext>();
        foreach (var dbContext in dbContexts)
        {
            await dbContext.MigrateAsync();
        }
    }

    public static void ExcludeMigrationByAttribute(this DbContext dbContext, ModelBuilder modelBuilder)
    {
        foreach (var property in dbContext.GetType().GetProperties())
        {
            var ignoreMigration = property.GetCustomAttribute<ExcludeMigrationAttribute>();
            if (ignoreMigration is null) continue;
            var clrType = property.PropertyType.GenericTypeArguments.FirstOrDefault();
            if (clrType is null) continue;
            modelBuilder.Entity(clrType).ToTable(property.Name, x => x.ExcludeFromMigrations());
        }
    }
}