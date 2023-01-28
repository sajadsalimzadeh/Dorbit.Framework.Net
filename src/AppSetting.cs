using Devor.Framework.Enums;

namespace Devor.Framework
{
    internal class AppSetting
    {
        public AppSettingGeo Geo { get; set; } = new AppSettingGeo();
        public AppSettingCaptcha Captcha { get; set; } = new AppSettingCaptcha();
    }

    internal class AppSettingGeo
    {
        public bool Enable { get; set; }
    }

    internal class AppSettingCaptcha
    {
        public int Length { get; set; }
        public string Pattern { get; set; }
        public CaptchaDificulty Dificulty { get; set; }
    }
}
