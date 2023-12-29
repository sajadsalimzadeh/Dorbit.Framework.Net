using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class Application : IApplication
{
    public byte[] Key { get; } = "a&hhmSmYQJUc8MP$mh7vf^!hvPV58nNE8S@JnK5H^QbZEcTabH"u8.ToArray();
}