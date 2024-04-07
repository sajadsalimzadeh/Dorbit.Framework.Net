using System;
using AutoMapper;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework;

public static class App
{
    private static IApplication _current;
    public static IApplication Current => _current ??= ServiceProvider.GetService<IApplication>();

    public static IServiceProvider ServiceProvider { get; internal set; }

    private static byte[] _key;
    public static byte[] Key => _key ??= Current?.Key;

    private static IMemoryCache _memoryCache;
    public static IMemoryCache MemoryCache => _memoryCache ??= ServiceProvider.GetService<IMemoryCache>();

    private static IMapper _mapper;
    public static IMapper Mapper => _mapper ??= ServiceProvider.GetService<IMapper>();

    private static AppSetting _setting;
    internal static AppSetting Setting => _setting ??= ServiceProvider.GetService<AppSetting>();
}