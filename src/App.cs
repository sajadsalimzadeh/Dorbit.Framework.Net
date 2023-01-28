using Devor.Framework.Database;
using Devor.Framework.Extensions;
using Devor.Framework.Models;
using Devor.Framework.Services;
using Devor.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security.Principal;

namespace Devor.Framework
{
    public static class App
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, Action<IServiceCollection, IConfiguration> action)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false).Build();

            action(services, configuration);

            return services;
        }

        public static IMvcBuilder AddDevor(this IServiceCollection services, Action<Configuration> action = null)
        {
            var configuration = new Configuration();
            action?.Invoke(configuration);

            services.TryAddSingleton(services);
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();

            services.RegisterServicesByAttribute(configuration.EntryAssembly);

            configuration.Logger?.Configure(services);
            services.AddScoped<IPrincipal>((sp) => sp.GetService<IHttpContextAccessor>()?.HttpContext?.User);

            var mvcBuilder = services.AddControllers()
                .AddApplicationPart(typeof(Controllers.CaptchasController).Assembly);
            services.TryAddSingleton(mvcBuilder);

            mvcBuilder.AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.BindConfiguration<AppSetting>("framework");

            services.AddDbContextPool<LogDbContext>(o =>
            {
                if (configuration.LogConnectionString is not null)
                {
                    o.UseSqlServer(configuration.LogConnectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(60));
                }
                else
                {
                    o.UseInMemoryDatabase("Framework");
                }
            });

            return mvcBuilder;
        }
    }
}
