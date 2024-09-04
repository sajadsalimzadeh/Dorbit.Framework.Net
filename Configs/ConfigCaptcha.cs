using Dorbit.Framework.Contracts;

namespace Dorbit.Framework.Configs;

public class ConfigCaptcha
{
    public int Length { get; set; } = 4;
    public string Pattern { get; set; } = "\\d";
    public CaptchaDificulty Difficulty { get; set; } = CaptchaDificulty.Normal;
}