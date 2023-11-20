using Dorbit.Attributes;
using Dorbit.Commands.Abstractions;
using Dorbit.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ICommand = Dorbit.Commands.Abstractions.ICommand;

namespace Dorbit.Commands;

[ServiceRegister]
public class MigrationCommand : Command
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationCommand(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public override IEnumerable<ICommand> GetSubCommands(Abstractions.ICommandContext context)
    {
        var dbContexts = _serviceProvider.GetServices<IDbContext>();
        yield return new MigrationCommandAll(dbContexts);
        foreach (var dbContext in dbContexts)
        {
            yield return new MigrationCommandItem(dbContext);
        }
    }

    public override string Message { get; }
    public override void Invoke(ICommandContext context)
    {
        throw new NotImplementedException();
    }
}
[ServiceRegister]
public class MigrationCommandAll : Command
{
    private readonly IEnumerable<IDbContext> dbContexts;

    public override string Message => "All";

    public MigrationCommandAll(IEnumerable<IDbContext> dbContexts)
    {
        this.dbContexts = dbContexts;
    }

    public override void Invoke(ICommandContext context)
    {
        foreach (var dbContext in dbContexts)
        {
            try
            {
                dbContext.Migrate();
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
        this._dbContext = dbContext;
    }

    public override void Invoke(ICommandContext context)
    {
        _dbContext.Migrate();
        context.Log($"{_dbContext.GetType().Name} Migrate");
    }
}