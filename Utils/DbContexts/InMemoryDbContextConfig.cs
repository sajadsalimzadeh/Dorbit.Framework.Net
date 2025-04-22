using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Utils.DbContexts;

public class InMemoryDbContextConfig(string databaseName) : DbContextConfig
{
    public string DatabaseName { get; set; } = databaseName;

    public override void Configure(DbContextOptionsBuilder builder)
    {
        builder.UseInMemoryDatabase(DatabaseName);
    }
}