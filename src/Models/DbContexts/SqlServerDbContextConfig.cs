using Microsoft.EntityFrameworkCore;

namespace Devor.Framework.Models.DbContexts;

public class SqlServerDbContextConfig : DbContextConfig
{
    public string ConnectionString { get; set; }

    public SqlServerDbContextConfig(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public override void Configure(DbContextOptionsBuilder builder)
    {
        builder.UseSqlServer(ConnectionString);
    }
}