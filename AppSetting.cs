using System.Collections.Generic;
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
    public AppSettingMessageProvider[] Providers { get; set; }
}

public class AppSettingMessageProvider
{
    public string Name { get; set; }
    public string Sender { get; set; }
    public string Username { get; set; }
    public ProtectedProperty ApiKey { get; set; }
    public ProtectedProperty Password { get; set; }
    public Dictionary<string, string> Templates { get; set; }

    //Email
    public string Server { get; set; }
    public short Port { get; set; }
}

internal class AppSettingSecurity
{
    public ProtectedProperty Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public short TimeoutInSecond { get; set; }
}