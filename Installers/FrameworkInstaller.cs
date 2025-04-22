using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Configs.Abstractions;
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
using IStartup = Dorbit.Framework.Services.Abstractions.IStartup;

namespace Dorbit.Framework.Installers;

public static class FrameworkInstaller
{
    public static IServiceCollection AddDorbitFramework<T>(this IServiceCollection services, Configs configs)
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

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                var patterns = (configs.AllowedOrigins ?? ["//localhost"]).Select(x => new Regex(x));
                policyBuilder
                    .SetIsOriginAllowed(origin => patterns.Any(x => x.IsMatch(origin)))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        services.RegisterServicesByAssembly(typeof(T).Assembly, configs.DependencyRegisterNamespaces.ToArray());

        services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>()?.HttpContext?.User);

        services.AddAutoMapper(typeof(FrameworkInstaller).Assembly);

        services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; });

        var frameworkDbContextConfiguration = configs.DbContextConfiguration ?? (builder => builder.UseInMemoryDatabase("Framework"));
        services.AddDbContext<FrameworkDbContext>(frameworkDbContextConfiguration);

        configs.ConfigFile?.Configure(services);
        configs.ConfigMessageProviders?.Configure(services);
        configs.ConfigLogRequest?.Configure(services);
        configs.ConfigCaptcha?.Configure(services);
        configs.ConfigGeo?.Configure(services);

        if (configs.ConfigSecurity is not null)
        {
            configs.ConfigSecurity.Configure(services);

            var securityAssembly = configs.ConfigSecurity.Configuration["Assembly"];
            if (!string.IsNullOrEmpty(securityAssembly))
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, securityAssembly);
                var assembly = Assembly.LoadFile(path);
                App.Security = new AppSecurityExternal(assembly);
            }
        }

        return services;
    }

    public class Configs(IConfiguration configuration)
    {
        public required List<string> DependencyRegisterNamespaces { get; init; }
        public List<string> AllowedOrigins { get; set; } = configuration.GetSection("AllowedOrigins").Get<List<string>>();

        public IConfig<ConfigFile> ConfigFile { get; init; } = configuration.GetConfig<ConfigFile>("File");
        public IConfig<ConfigMessageProviders> ConfigMessageProviders { get; init; } = configuration.GetConfig<ConfigMessageProviders>("MessageProviders");
        public IConfig<ConfigFrameworkSecurity> ConfigSecurity { get; init; } = configuration.GetConfig<ConfigFrameworkSecurity>("Security");
        public IConfig<ConfigLogRequest> ConfigLogRequest { get; init; } = configuration.GetConfig<ConfigLogRequest>("LogRequest");
        public IConfig<ConfigCaptcha> ConfigCaptcha { get; init; } = configuration.GetConfig<ConfigCaptcha>("Captcha");
        public IConfig<ConfigGeo> ConfigGeo { get; init; } = configuration.GetConfig<ConfigGeo>("Geo");

        public Action<DbContextOptionsBuilder> DbContextConfiguration { get; init; }
    }

    public static WebApplicationBuilder UseDorbitSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
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

        app.UseSwagger();
        app.UseSwaggerUI(o => o.UseDefaultOptions("Mobicar.Shared.CoreServer API v1"));

        app.UseExceptionHandler("/Error");
        app.UseHsts();

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
}