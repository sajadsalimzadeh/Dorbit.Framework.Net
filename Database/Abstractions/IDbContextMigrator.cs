using System.Threading.Tasks;

namespace Dorbit.Framework.Database.Abstractions;

public interface IDbContextMigrator
{
    Task MigrateAsync();
}