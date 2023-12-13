using Dorbit.Framework.Attributes;
using Dorbit.Framework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Database;

[ServiceRegister]
public class LogDbContext : EfDbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("log");
    }

    public DbSet<EntityLog> Logs { get; set; }
}