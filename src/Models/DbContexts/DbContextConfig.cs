using Devor.Framework.Enums;
using Microsoft.EntityFrameworkCore;

namespace Devor.Framework.Models.DbContexts;

public abstract class DbContextConfig
{
    public abstract void Configure(DbContextOptionsBuilder builder);
}