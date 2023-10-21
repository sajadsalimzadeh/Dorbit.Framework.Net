using Microsoft.EntityFrameworkCore;

namespace Dorbit.Utils.DbContexts;

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