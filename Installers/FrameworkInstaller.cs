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

    public class Configs
    {
        public required Assembly EntryAssembly { get; init; }
        public required List<string> DependencyRegisterNamespaces { get; init; }
        public List<string> AllowedOrigins { get; set; }

        public IConfig<ConfigFile> ConfigFile { get; init; }
        public IConfig<ConfigMessageProviders> ConfigMessageProviders { get; init; }
        public IConfig<ConfigFrameworkSecurity> ConfigSecurity { get; init; }
        public IConfig<ConfigLogRequest> ConfigLogRequest { get; init; }
        public IConfig<ConfigCaptcha> ConfigCaptcha { get; init; }
        public IConfig<ConfigGeo> ConfigGeo { get; init; }

        public Action<DbContextOptionsBuilder> DbContextConfiguration { get; init; }

        public Configs(IConfiguration configuration)
        {
            ConfigFile = configuration.GetConfig<ConfigFile>("File");
            ConfigMessageProviders = configuration.GetConfig<ConfigMessageProviders>("MessageProviders");
            ConfigSecurity = configuration.GetConfig<ConfigFrameworkSecurity>("Security");
            ConfigLogRequest = configuration.GetConfig<ConfigLogRequest>("LogRequest");
            ConfigCaptcha = configuration.GetConfig<ConfigCaptcha>("Captcha");
            ConfigGeo = configuration.GetConfig<ConfigGeo>("Geo");

            AllowedOrigins = configuration.GetSection("AllowedOrigins").Get<List<string>>();
        }
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