using Microsoft.EntityFrameworkCore;

namespace Devor.Framework.Models.DbContexts;

public class InMemoryDbContextConfig : DbContextConfig
{
    public string DatabaseName { get; set; }

    public InMemoryDbContextConfig(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public override void Configure(DbContextOptionsBuilder builder)
    {
        builder.UseInMemoryDatabase(DatabaseName);
    }
}