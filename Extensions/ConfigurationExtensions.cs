using Dorbit.Framework.Configs;
using Dorbit.Framework.Configs.Abstractions;
using Dorbit.Framework.Contracts.Cryptograpy;
using Dorbit.Framework.Installers;
using Microsoft.Extensions.Configuration;

namespace Dorbit.Framework.Extensions;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringWithPassword(this IConfiguration configuration, string name, byte[] encryptionKey)
    {
        var connectionString = configuration.GetConnectionString(name);
        var connectionStringPasswords = configuration.GetSection($"ConnectionStringPasswords");
        var password = connectionStringPasswords.GetSection(name).Get<ProtectedProperty>();
        return connectionString?.Replace("{password}", password.GetDecryptedValue(encryptionKey));
    }

    public static IConfig<T> GetConfig<T>(this IConfiguration configuration, string key) where T : class
    {
        return new Config<T>(configuration.GetSection(key));
    }
}