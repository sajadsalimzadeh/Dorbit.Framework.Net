using Devor.Framework.Attributes;
using Devor.Framework.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Devor.Framework.Database
{
    [ServiceRegisterar]
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
}
