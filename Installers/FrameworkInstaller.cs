using System.Security.Principal;
using System.Text.Json;
using Dorbit.Framework.Database;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        configuration.LogDbContextConfiguration ??= builder => builder.UseInMemoryDatabase("Logger");
        services.AddDbContext<LogDbContext>(configuration.LogDbContextConfiguration);

        return services;
    }

    public static WebApplicationBuilder ConfigureDorbitSerilog(this WebApplicationBuilder builder)
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

    public static WebApplication RunDorbit(this WebApplication app, string[] args)
    {
        App.ServiceProvider = app.Services;
        
        if (args.Contains("cli"))
        {
            app.RunCli();
        }
        else
        {
            app.Run();
        }
        return app;
    }

    public class Configuration
    {
        public required List<string> DependencyRegisterNamespaces { get; init; }
        public Action<DbContextOptionsBuilder> LogDbContextConfiguration { get; set; }
    }
}