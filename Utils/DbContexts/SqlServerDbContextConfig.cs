using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Utils.DbContexts;

public class SqlServerDbContextConfig(string connectionString) : DbContextConfig
{
    public string ConnectionString { get; set; } = connectionString;

    public override void Configure(DbContextOptionsBuilder builder)
    {
        builder.UseSqlServer(ConnectionString);
    }
}