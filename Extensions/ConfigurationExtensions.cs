using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Configs.Abstractions;
using Dorbit.Framework.Contracts.Cryptograpy;
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

    public static object ToObject(this IConfiguration configuration)
    {
        return BuildObject(configuration);
    }

    private static object BuildObject(IConfiguration config)
    {
        var children = config.GetChildren().ToList();

        // Leaf node
        if (!children.Any())
        {
            var value = config is IConfigurationSection section
                ? section.Value
                : null;

            return ParseValue(value);
        }

        var dictionary = new Dictionary<string, object>();

        foreach (var child in children)
        {
            dictionary[child.Key] = BuildObject(child);
        }

        return dictionary;
    }

    private static object ParseValue(string? value)
    {
        if (value == null)
            return "";

        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        if (int.TryParse(value, out var intValue))
            return intValue;

        if (double.TryParse(value, out var doubleValue))
            return doubleValue;

        return value;
    }
}