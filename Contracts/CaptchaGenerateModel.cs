namespace Dorbit.Framework.Contracts;

public class CaptchaGenerateModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public string Pattern { get; set; } = "0123456789";
    public CaptchaDificulty Dificulty { get; set; }
}