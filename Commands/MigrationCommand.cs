using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ICommand = Dorbit.Framework.Commands.Abstractions.ICommand;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class MigrationCommand : Command
{
    private readonly IServiceProvider _serviceProvider;

    public override string Message => "Migrate";

    public MigrationCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override IEnumerable<ICommand> GetSubCommands(ICommandContext context)
    {
        var dbContexts = _serviceProvider.GetServices<IDbContext>();
        yield return new MigrationCommandAll(dbContexts);
        foreach (var dbContext in dbContexts)
        {
            yield return new MigrationCommandItem(dbContext);
        }
    }
    
    public override Task Invoke(ICommandContext context)
    {
        throw new NotImplementedException();
    }
}
[ServiceRegister]
public class MigrationCommandAll : Command
{
    private readonly IEnumerable<IDbContext> _dbContexts;

    public override string Message => "All";

    public MigrationCommandAll(IEnumerable<IDbContext> dbContexts)
    {
        _dbContexts = dbContexts;
    }

    public override async Task Invoke(ICommandContext context)
    {
        foreach (var dbContext in _dbContexts)
        {
            try
            {
                await dbContext.MigrateAsync();
                context.Log($"{dbContext.GetType().Name} Migrate");
            }
            catch (Exception ex)
            {
                context.Error(ex.Message);
            }
        }
    }
}
internal class MigrationCommandItem : Command
{
    private readonly IDbContext _dbContext;

    public override string Message => _dbContext.GetType().Name;

    public MigrationCommandItem(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task Invoke(ICommandContext context)
    {
        await _dbContext.MigrateAsync();
        context.Log($"{_dbContext.GetType().Name} Migrate");
    }
}