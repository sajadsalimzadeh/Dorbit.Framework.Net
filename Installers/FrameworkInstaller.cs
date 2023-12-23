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

        if (!configuration.DependencyRegisterNamespaces.Contains("Dorbit"))
        {
            configuration.DependencyRegisterNamespaces.Add("Dorbit");
        }

        services.RegisterServicesByAssembly(configuration.DependencyRegisterNamespaces.ToArray());

        services.AddScoped<IPrincipal>((sp) => sp.GetService<IHttpContextAccessor>()?.HttpContext?.User);

        services.AddAutoMapper(typeof(FrameworkInstaller).Assembly);
        services.AddControllers(typeof(FrameworkInstaller).Assembly)
            .AddJsonOptions(options => { options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; });

        configuration.Logger?.Configure(services);
        services.AddDbContext<LogDbContext>(configuration.LogDbContextConfiguration);

        return services;
    }

    public static IServiceCollection AddOData(IServiceCollection services)
    {
        services.AddControllers(typeof(FrameworkInstaller).Assembly)
            .AddODataDefault();
        
        return services;
    }

    public static WebApplication Run(this WebApplication app, string[] args)
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
        public required Action<DbContextOptionsBuilder> LogDbContextConfiguration { get; init; }

        public IConfigurationLogger Logger { get; set; }
        public string LogConnectionString { get; init; }
    }
}