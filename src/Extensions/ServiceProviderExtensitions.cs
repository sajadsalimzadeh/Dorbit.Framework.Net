using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Devor.Framework.Extensions
{
    public static class ServiceProviderExtensitions
    {
        private static AppSettingGlobal AppSettingGlobal;

        private static string GetAppSettingsFilePath(string moduleName = null, string environment = null)
        {
            var filename = $"appsettings.";
            if (!string.IsNullOrEmpty(moduleName)) filename += $"{moduleName}.";
            if (!string.IsNullOrEmpty(environment)) filename += environment + ".";
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename + "json");
        }

        public static IConfigurationRoot BindConfiguration(this IServiceCollection services, string name = null)
        {
            if (AppSettingGlobal is null)
            {
                var appsettingsMainFilename = GetAppSettingsFilePath("global");
                if (!File.Exists(appsettingsMainFilename)) throw new Exception($"{appsettingsMainFilename} not found");
                AppSettingGlobal = JsonConvert.DeserializeObject<AppSettingGlobal>(File.ReadAllText(appsettingsMainFilename));
                services.AddSingleton(AppSettingGlobal);
            }

            string filename = default;
            foreach (var environment in AppSettingGlobal.Environments)
            {
                var tmp = GetAppSettingsFilePath(name, environment);
                if (File.Exists(tmp)) { filename = tmp; break; }
            }
            if (filename is null) throw new Exception($"appsetting not found for module {name}");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile(filename, false).Build();
        }

        public static T BindConfiguration<T>(this IServiceCollection services, string name = null) where T : class
        {
            var settings = services.BindConfiguration(name);
            var appSettings = Activator.CreateInstance<T>();
            settings.Bind(appSettings);
            services.AddSingleton(appSettings);
            return appSettings;
        }

        public static bool IsDevelopment(this IServiceCollection services)
        {
#if !DEBUG
            return false;
#endif
            return true;
        }
    }
}
