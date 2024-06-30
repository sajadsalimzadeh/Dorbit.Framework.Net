using System;
using System.Threading;
using AutoMapper;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework;

public static class App
{
    public static Thread MainThread { get; internal set; }
    public static IServiceProvider ServiceProvider { get; internal set; }
    
    private static IApplication _current;
    public static IApplication Current => _current ??= ServiceProvider.GetService<IApplication>();

    private static IMemoryCache _memoryCache;
    public static IMemoryCache MemoryCache => _memoryCache ??= ServiceProvider.GetService<IMemoryCache>();

    private static IMapper _mapper;
    public static IMapper Mapper => _mapper ??= ServiceProvider.GetService<IMapper>();
    
    public static AppSecurity Security { get; } = new();
}
