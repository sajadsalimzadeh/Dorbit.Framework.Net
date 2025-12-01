using System;
using System.Collections.Concurrent;

namespace Dorbit.Framework.Hubs.Abstractions;

public interface INotificationHub
{
    BaseHub Hub { get; }
    ConcurrentDictionary<Guid, string> Connections { get; }
}