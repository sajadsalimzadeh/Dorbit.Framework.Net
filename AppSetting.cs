using Dorbit.Framework.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework;

internal class AppSetting
{
    private static AppSetting _current;
    public static AppSetting Current => _current ??= App.ServiceProvider.GetService<AppSetting>();
        
    public AppSettingGeo Geo { get; set; } = new ();
    public AppSettingCaptcha Captcha { get; set; } = new ();
    public AppSettingMessage Message { get; set; } = new ();
}

internal class AppSettingGeo
{
    public bool Enable { get; set; }
}

internal class AppSettingCaptcha
{
    public int Length { get; set; }
    public string Pattern { get; set; }
    public CaptchaDificulty Difficulty { get; set; }
}

internal class AppSettingMessage
{
    public List<IConfiguration> Providers { get; set; }
}

internal class AppSettingMessageSmsProvider
{
    public string Name { get; set; }
    public string ApiToken { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

internal class AppSettingMessageEmailProvider
{
}