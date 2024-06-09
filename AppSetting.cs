using System.Collections.Generic;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework;

internal class AppSetting
{
    public AppSettingGeo Geo { get; set; } = new();
    public AppSettingCaptcha Captcha { get; set; } = new();
    public AppSettingMessage Message { get; set; } = new();
    public AppSettingSecurity Security { get; set; } = new();
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
    public ConfigMessageProvider[] Providers { get; set; }
}

internal class AppSettingSecurity
{
    public ProtectedProperty Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int TimeoutInSecond { get; set; }
}