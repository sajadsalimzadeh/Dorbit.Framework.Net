using Microsoft.EntityFrameworkCore;

namespace Dorbit.Utils.DbContexts;

public abstract class DbContextConfig
{
    public abstract void Configure(DbContextOptionsBuilder builder);
}