using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Utils.DbContexts;

public abstract class DbContextConfig
{
    public abstract void Configure(DbContextOptionsBuilder builder);
}