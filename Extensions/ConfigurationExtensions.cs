﻿using Dorbit.Framework.Models.Cryptographies;
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
}