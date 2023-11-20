using System.Security.Principal;
using System.Text.Json;
using Dorbit.Database;
using Dorbit.Extensions;
using Dorbit.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dorbit.Installers;

public static class FrameworkInstaller
{
    public static IServiceProvider ServiceProvider { get; private set; }

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
        services.AddControllers(typeof(FrameworkInstaller).Assembly);
        
        configuration.Logger?.Configure(services);
        services.AddDbContext<LogDbContext>(configuration.LogDbContextConfiguration);

        return services;
    }

    public static IApplicationBuilder UseDorbitFramework(this IApplicationBuilder app)
    {
        ServiceProvider = app.ApplicationServices;

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