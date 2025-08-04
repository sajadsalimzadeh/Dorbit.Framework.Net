using System;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Database;

[ServiceRegister]
public class FrameworkDbContext(DbContextOptions<FrameworkDbContext> options, IServiceProvider serviceProvider)
    : EfDbContext(options, serviceProvider)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("framework");
    }

    public DbSet<ChangeLog> Logs { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
}