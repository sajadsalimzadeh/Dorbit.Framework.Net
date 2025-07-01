using System;
using System.Threading;
using AutoMapper;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework;

public static class App
{
    public static bool InMemory { get; set; }
    public static Thread MainThread { get; internal set; }
    public static CancellationToken MainCancellationToken { get; internal set; }
    public static IServiceProvider ServiceProvider { get; internal set; }
    public static IApplication Current { get; internal set; }
    public static IMemoryCache MemoryCache { get; internal set; }
    public static IMapper Mapper { get; internal set; }
    public static IAppSecurity Security { get; internal set; }
}