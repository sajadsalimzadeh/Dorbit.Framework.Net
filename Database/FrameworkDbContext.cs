using System;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Database;

[ServiceRegister]
public class FrameworkDbContext : EfDbContext
{
    public FrameworkDbContext(DbContextOptions<FrameworkDbContext> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("frm");
    }

    public DbSet<EntityLog> Logs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}