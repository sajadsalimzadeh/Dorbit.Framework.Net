using Devor.Framework.Enums;

namespace Devor.Framework.Models
{
    public class CaptchaGenerateModel
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Length { get; set; }
        public string Pattern { get; set; }
        public CaptchaDificulty Dificulty { get; set; }
    }
}
