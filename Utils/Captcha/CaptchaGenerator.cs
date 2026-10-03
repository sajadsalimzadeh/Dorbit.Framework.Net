using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System;
using System.IO;
using System.Linq;
using Dorbit.Framework.Contracts;

namespace Dorbit.Framework.Utils.Captcha;

public class CaptchaGenerator
{
    public CaptchaDificulty Difficulty { get; set; } = CaptchaDificulty.Normal;
    public int Width { get; set; } = 80;
    public int Height { get; set; } = 200;
    
    private static readonly FontFamily[] FontFamilies = SystemFonts.Families.ToArray();
    private static readonly FontStyle[] FontStyles = [FontStyle.Bold, FontStyle.BoldItalic, FontStyle.Italic, FontStyle.Regular];
    private static readonly TextDecorations[] TextDecorationItems = [TextDecorations.None, TextDecorations.Overline, TextDecorations.Strikeout, TextDecorations.Underline];
    
    private int GetRotation()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => 0,
            CaptchaDificulty.Easy => Random.Shared.Next(-10, 10),
            CaptchaDificulty.Normal => Random.Shared.Next(-30, 30),
            CaptchaDificulty.Hard => Random.Shared.Next(-50, 50),
            _ => Random.Shared.Next(-60, 60),
        };
    }

    private int GetFontSize()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => Random.Shared.Next((int)(Height * .7), (int)(Height * .9)),
            CaptchaDificulty.Easy => Random.Shared.Next((int)(Height * .6), (int)(Height * .8)),
            CaptchaDificulty.Normal => Random.Shared.Next((int)(Height * .5), (int)(Height * .7)),
            CaptchaDificulty.Hard => Random.Shared.Next((int)(Height * .4), (int)(Height * .6)),
            _ => Random.Shared.Next((int)(Height * .3), (int)(Height * .5)),
        };
    }

    private FontStyle GetFontStyle()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => FontStyles[Random.Shared.Next(0, 1)],
            CaptchaDificulty.Easy => FontStyles[Random.Shared.Next(0, 2)],
            _ => FontStyles[Random.Shared.Next(0, 3)]
        };
    }

    private TextDecorations GetTextDecoration()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => TextDecorationItems[Random.Shared.Next(0, 1)],
            CaptchaDificulty.Easy => TextDecorationItems[Random.Shared.Next(0, 2)],
            _ => TextDecorationItems[Random.Shared.Next(0, 3)]
        };
    }

    private FontFamily GetFontFamily()
    {
        switch (Difficulty)
        {
            case CaptchaDificulty.VeryEasy: return FontFamilies[0];
            case CaptchaDificulty.Easy: return FontFamilies[Random.Shared.Next(0, 1)];
            case CaptchaDificulty.Normal: return FontFamilies[Random.Shared.Next(0, 2)];
            case CaptchaDificulty.Hard: return FontFamilies[Random.Shared.Next(0, 4)];
            case CaptchaDificulty.VeryHard:
            case CaptchaDificulty.None:
            default:
                return FontFamilies[Random.Shared.Next(0, 5)];
        }
    }

    private Color GetColor()
    {
        return Color.FromPixel(new Rgb24((byte)Random.Shared.Next(0, 200), (byte)Random.Shared.Next(0, 200), (byte)Random.Shared.Next(0, 200)));
    }

    public Image<Rgba32> Generate(string text)
    {
        var font = GetFontFamily();
        var image = new Image<Rgba32>(Width, Height);
        image.Mutate(ctx => ctx.Paint(canvas =>
        {
            for (int i = 0, length = text.Length, unit = Width / (length + 2); i < length; i++)
            {
                var x = unit * (i + 1);
                var y = Height / 2 - 20;

                var color = GetColor();
                var fontSize = GetFontSize();
                var fontStyle = GetFontStyle();
                var textDecoration = GetTextDecoration();
                
                RichTextOptions textOptions = new(SystemFonts.CreateFont(font.Name, fontSize, fontStyle))
                {
                    Origin = new PointF(x, y),
                    WrappingLength = 1040,
                    TextRuns =
                    [
                        new RichTextRun
                        {
                            Start = 0,
                            End = 6,
                            TextDecorations = textDecoration
                        }
                    ]
                };
                
                canvas.DrawText(textOptions, text[i].ToString(), Brushes.Solid(color), pen: null);
            }
            
        }));

        return image;
    }
    
    public string GenerateBase64(string text)
    {
        using var image = Generate(text);
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return Convert.ToBase64String(ms.ToArray());
    }
}