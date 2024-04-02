using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Database;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Middlewares;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Installers;

public static class FrameworkInstaller
{
    public static IServiceCollection AddDorbitFramework(this IServiceCollection services, Configuration configuration)
    {
        services.BindConfiguration<AppSetting>();

        services.TryAddSingleton(services);
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSerilog();

        if (!configuration.DependencyRegisterNamespaces.Contains("Dorbit"))
        {
            configuration.DependencyRegisterNamespaces.Add("Dorbit");
        }

        services.RegisterServicesByAssembly(configuration.DependencyRegisterNamespaces.ToArray());

        services.AddScoped<IPrincipal>((sp) => sp.GetService<IHttpContextAccessor>()?.HttpContext?.User);

        services.AddAutoMapper(typeof(FrameworkInstaller).Assembly);

        services.AddControllers(typeof(FrameworkInstaller).Assembly)
            .AddJsonOptions(options => { options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; });

        services.AddDbContext<FrameworkDbContext>(configuration.FrameworkDbContextConfiguration);

        return services;
    }

    public static IHostBuilder ConfigureDorbitSerilog(this IHostBuilder builder)
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

    public static WebApplication BuildWithDorbit(this WebApplicationBuilder builder)
    {
        var app = builder.Build();
        App.ServiceProvider = app.Services;
        var df = new DefaultFilesOptions();
        df.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(df);
        app.UseStaticFiles();
        app.UseCors();
        app.UseRouting();
        app.UseMiddleware<AuthMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<CancellationTokenMiddleware>();
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
            if (args.Contains("cli")) await app.RunCliAsync();
            else await app.RunWithStartupsAsync();
        }
        else if (args.Contains("run"))
        {
            await app.RunWithStartupsAsync();
        }
        else
        {
            await app.RunCliAsync();
        }
    }

    public class Configuration
    {
        public required List<string> DependencyRegisterNamespaces { get; init; }
        public Action<DbContextOptionsBuilder> FrameworkDbContextConfiguration { get; set; }
    }
}