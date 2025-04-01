using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ICommand = Dorbit.Framework.Commands.Abstractions.ICommand;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class MigrationCommand(IServiceProvider serviceProvider) : Command
{
    public override string Message => "Migrate";

    public override IEnumerable<ICommand> GetSubCommands(ICommandContext context)
    {
        var dbContexts = serviceProvider.GetServices<IDbContext>();
        yield return new MigrationCommandAll(dbContexts);
        foreach (var dbContext in dbContexts)
        {
            yield return new MigrationCommandItem(dbContext);
        }
    }

    public override Task InvokeAsync(ICommandContext context)
    {
        throw new NotImplementedException();
    }
}

[ServiceRegister]
public class MigrationCommandAll(IEnumerable<IDbContext> dbContexts) : Command
{
    public override bool IsRoot { get; } = false;

    public override string Message => "Migrate All";

    public override async Task InvokeAsync(ICommandContext context)
    {
        context.Log($"\n==========Migrating==========\n");
        foreach (var dbContext in dbContexts)
        {
            try
            {
                await dbContext.MigrateAsync();
                context.Log($"{dbContext.GetType().Name} Migrated\n");
            }
            catch (Exception ex)
            {
                context.Error(ex.Message + "\n");
            }
        }
    }
}

internal class MigrationCommandItem(IDbContext dbContext) : Command
{
    public override string Message => dbContext.GetType().Name;

    public override async Task InvokeAsync(ICommandContext context)
    {
        await dbContext.MigrateAsync();
        context.Log($"{dbContext.GetType().Name} Migrate");
    }
}