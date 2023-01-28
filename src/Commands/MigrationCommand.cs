using Devor.Framework.Attributes;
using Devor.Framework.Database.Abstractions;
using Devor.Framework.Services;
using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Devor.Framework.Commands
{
    [ServiceRegisterar]
    public class MigrationCommand : Command
    {
        private readonly IServiceProvider serviceProvider;

        public MigrationCommand(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override IEnumerable<ICommand> GetSubCommands()
        {
            var dbContexts = serviceProvider.GetServices<IDbContext>();
            yield return new MigrationCommandAll(dbContexts);
            foreach (var dbContext in dbContexts)
            {
                yield return new MigrationCommandItem(dbContext);
            }
        }

        public override void Invoke(ICommandContext context)
        { }
    }
    [ServiceRegisterar]
    public class MigrationCommandAll : Command
    {
        private readonly IEnumerable<IDbContext> dbContexts;

        public override string Name => "All";

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
        private readonly IDbContext dbContext;

        public override string Name => dbContext.GetType().Name;

        public MigrationCommandItem(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override void Invoke(ICommandContext context)
        {
            dbContext.Migrate();
            context.Log($"{dbContext.GetType().Name} Migrate");
        }
    }
}
