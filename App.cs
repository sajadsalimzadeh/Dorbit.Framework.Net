using System.Security.Principal;
using AutoMapper;
using Dorbit.Database;
using Dorbit.Extensions;
using Dorbit.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dorbit;

public static class App
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static IMvcBuilder AddDorbit(this IServiceCollection services, Action<Configuration> action = null)
    {
        var configuration = new Configuration();
        action?.Invoke(configuration);

        services.TryAddSingleton(services);
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();

        services.RegisterServicesByAssembly(new[] { "Dorbit" });

        configuration.Logger?.Configure(services);
        services.AddScoped<IPrincipal>((sp) => sp.GetService<IHttpContextAccessor>()?.HttpContext?.User);

        var mvcBuilder = services.AddControllers()
            .AddApplicationPart(typeof(Controllers.CaptchasController).Assembly);
        services.TryAddSingleton(mvcBuilder);

        services.BindConfiguration<AppSetting>();

        services.AddDbContextPool<LogDbContext>(o =>
        {
            if (configuration.LogConnectionString is not null)
            {
                o.UseNpgsql(configuration.LogConnectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(60));
            }
            else
            {
                o.UseInMemoryDatabase("Framework");
            }
        });

        return mvcBuilder;
    }

    public static IApplicationBuilder UseDorbit(this IApplicationBuilder app)
    {
        ServiceProvider = app.ApplicationServices;

        return app;
    }
}