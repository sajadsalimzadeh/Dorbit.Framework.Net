using Devor.Framework.Enums;
using Devor.Framework.Models.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Devor.Framework.Extensions;

public static class DbContextExtensions
{

    public static void UseDbContextConfig(this DbContextOptionsBuilder builder, DbContextConfig config)
    {
        config.Configure(builder);
    }
}