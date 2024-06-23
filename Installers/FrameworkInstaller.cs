using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Database;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Middlewares;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

        services.BindConfiguration<AppSetting>();
        services.TryAddSingleton(services);
        services.AddResponseCaching();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSerilog();

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

        if (configs.MessageConfig is not null)
        {
            services.Configure<ConfigMessage>(configs.MessageConfig);
        }

        App.ServiceProvider = services.BuildServiceProvider();

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

    public static WebApplication UseDorbit(this WebApplication app)
    {
        App.ServiceProvider = app.Services;
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(defaultFilesOptions);
        app.UseStaticFiles();
        app.UseCors();
        app.UseRouting();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => o.UseDefaultOptions("Mobicar.Shared API v1"));
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
        public Action<DbContextOptionsBuilder> FrameworkDbContextConfiguration { get; set; }

        public IConfiguration MessageConfig { get; init; }
    }
}