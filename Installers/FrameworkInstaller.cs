using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Database;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Middlewares;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Services.AppSecurities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Installers;

public static class FrameworkInstaller
{
    public static IServiceCollection AddDorbitFramework(this IServiceCollection services, Configs configs)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        App.MainThread = Thread.CurrentThread;

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        
        var appSettingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.custom.json");
        
        if (!File.Exists(appSettingPath))
        {
            File.WriteAllText(appSettingPath, "{}");
        }

        services.BindConfiguration<AppSetting>();
        services.TryAddSingleton(services);
        services.AddResponseCaching();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSerilog();
        services.AddSwaggerGen();

        if (!configs.DependencyRegisterNamespaces.Contains("Dorbit"))
        {
            configs.DependencyRegisterNamespaces.Add("Dorbit");
        }

        services.RegisterServicesByAssembly(configs.EntryAssembly, configs.DependencyRegisterNamespaces.ToArray());

        services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>()?.HttpContext?.User);

        services.AddAutoMapper(typeof(FrameworkInstaller).Assembly);

        services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; });

        services.AddDbContext<FrameworkDbContext>(configs.FrameworkDbContextConfiguration);

        if (configs.ConfigMessageProvider is not null) services.Configure<ConfigMessageProviders>(configs.ConfigMessageProvider);
        if (configs.ConfigSecurity is not null)
        {
            services.Configure<ConfigSecurity>(configs.ConfigSecurity);

            var securityAssembly = configs.ConfigSecurity["Assembly"];
            if (!string.IsNullOrEmpty(securityAssembly))
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, securityAssembly);
                var assembly = Assembly.LoadFile(path);
                App.Security = new AppSecurityExternal(assembly);
            }
        }

        return services;
    }

    public static IHostBuilder UseDorbitSerilog(this IHostBuilder builder)
    {
        builder.UseSerilog((hostingContext, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

        return builder;
    }

    public static IServiceCollection AddOData(IServiceCollection services)
    {
        services.AddControllers(typeof(FrameworkInstaller).Assembly)
            .AddODataDefault();

        return services;
    }

    public static WebApplication BuildDorbit(this WebApplicationBuilder builder)
    {
        App.ServiceProvider = builder.Services.BuildServiceProvider();
        App.Current = App.ServiceProvider.GetRequiredService<IApplication>();
        App.MemoryCache = App.ServiceProvider.GetRequiredService<IMemoryCache>();
        App.Mapper = App.ServiceProvider.GetRequiredService<IMapper>();
        App.Security ??= new AppSecurityInternal(App.Current.EncryptionKey);
        return builder.Build();
    }

    public static WebApplication UseDorbit(this WebApplication app)
    {
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(defaultFilesOptions);
        app.UseStaticFiles();
        app.UseCors();
        app.UseRouting();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => o.UseDefaultOptions("Mobicar.Shared.CoreServer API v1"));
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseMiddleware<AuthMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<CancellationTokenMiddleware>();
        app.UseResponseCaching();
        return app;
    }

    public static async Task RunWithStartupsAsync(this WebApplication app)
    {
        var startups = app.Services.GetServices<IStartup>();
        foreach (var startup in startups)
        {
            await startup.RunAsync();
        }

        await app.RunAsync();
    }

    public static async Task RunDorbitAsync(this WebApplication app, string[] args)
    {
        if (app.Environment.IsDevelopment())
        {
            if (args.Contains("run"))
            {
                await app.MigrateAll();
                await app.RunWithStartupsAsync();
            }
            else
            {
                await app.RunCliAsync();
            }
        }
        else
        {
            if (args.Contains("cli"))
            {
                await app.RunCliAsync();
            }
            else
            {
                await app.MigrateAll();
                await app.RunWithStartupsAsync();
            }
        }
    }

    public class Configs
    {
        public required Assembly EntryAssembly { get; init; }
        public required List<string> DependencyRegisterNamespaces { get; init; }
        public Action<DbContextOptionsBuilder> FrameworkDbContextConfiguration { get; init; }

        public IConfiguration ConfigMessageProvider { get; init; }
        public IConfiguration ConfigSecurity { get; init; }
    }
}