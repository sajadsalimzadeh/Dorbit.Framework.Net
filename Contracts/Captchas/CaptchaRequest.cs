using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Contracts.Captchas;

public class CaptchaRequest
{
    [Range(10, 300)]
    public int Width { get; set; }

    [Range(10, 200)]
    public int Height { get; set; }
}