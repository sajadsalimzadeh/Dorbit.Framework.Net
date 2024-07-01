using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class DorbitApplication : IApplication
{
    public byte[] EncryptionKey { get; } = "xZncxnz4Kct2UEVJdF1oisdVZOYb6KGqiHO2t9RROMQa5a5C1a"u8.ToArray();
}