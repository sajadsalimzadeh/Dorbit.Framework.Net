using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System;
using System.IO;
using Dorbit.Framework.Contracts;
using Fare;

namespace Dorbit.Framework.Utils.Captcha;

public class CaptchaGenerator
{
    public CaptchaDificulty Difficulty { get; set; } = CaptchaDificulty.Normal;
    public int Width { get; set; } = 80;
    public int Height { get; set; } = 200;
    public int Length { get; set; } = 6;
    public string Pattern { get; set; } = "[0-9A-Z]";
    
    private static readonly string[] FontNames =
    {
        "Comic Sans MS",
        "Arial",
        "Times New Roman",
        "Georgia",
        "Verdana",
        "Geneva"
    };
    
    public string GenerateBase64(string text, int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        image.Mutate(ctx =>
        {
            ctx.Fill(Color.White);
            ctx.DrawText(text, SystemFonts.CreateFont("Arial", 24), Color.Black, new PointF(10, 10));
            ctx.DrawLine(Color.Gray, 1, new PointF(0, 10), new PointF(width, 40), new PointF(0, 40), new PointF(width, 10));
        });

        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return Convert.ToBase64String(ms.ToArray());
    }
    

    private int GetRotation(Random rnd)
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => 0,
            CaptchaDificulty.Easy => rnd.Next(-10, 10),
            CaptchaDificulty.Normal => rnd.Next(-30, 30),
            CaptchaDificulty.Hard => rnd.Next(-50, 50),
            _ => rnd.Next(-60, 60),
        };
    }

    private int GetFontSize(Random rnd)
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => 20,
            CaptchaDificulty.Easy => rnd.Next(20, 25),
            CaptchaDificulty.Normal => rnd.Next(15, 25),
            CaptchaDificulty.Hard => rnd.Next(10, 30),
            _ => rnd.Next(10, 35),
        };
    }

    private FontStyle GetFontStyle()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => FontStyle.Bold,
            CaptchaDificulty.Easy => FontStyle.BoldItalic,
            CaptchaDificulty.Normal => FontStyle.Italic,
            _ => FontStyle.Regular
        };
    }

    private string GetFontName(Random rnd)
    {
        switch (Difficulty)
        {
            case CaptchaDificulty.VeryEasy: return FontNames[0];
            case CaptchaDificulty.Easy: return FontNames[rnd.Next(0, 1)];
            case CaptchaDificulty.Normal: return FontNames[rnd.Next(0, 2)];
            case CaptchaDificulty.Hard: return FontNames[rnd.Next(0, 4)];
            case CaptchaDificulty.VeryHard:
            case CaptchaDificulty.None:
            default:
                return FontNames[rnd.Next(0, 5)];
        }
    }

    private Color GetColor(Random rnd)
    {
        switch (Difficulty)
        {
            case CaptchaDificulty.VeryEasy: return Color.FromRgb(0, 0, 0);
            case CaptchaDificulty.Easy: return Color.FromRgb((byte)rnd.Next(0, 30), (byte)rnd.Next(0, 30), (byte)rnd.Next(0, 30));
            case CaptchaDificulty.Normal: return Color.FromRgb((byte)rnd.Next(0, 60), (byte)rnd.Next(0, 60), (byte)rnd.Next(0, 60));
            case CaptchaDificulty.Hard: return Color.FromRgb((byte)rnd.Next(0, 90), (byte)rnd.Next(0, 90), (byte)rnd.Next(0, 90));
            case CaptchaDificulty.VeryHard:
            case CaptchaDificulty.None:
            default:
                return Color.FromRgb((byte)rnd.Next(0, 150), (byte)rnd.Next(0, 150), (byte)rnd.Next(0, 150));
        }
    }

    public Image<Rgba32> Generate(string text)
    {
        var rnd = new Random();
        var image = new Image<Rgba32>(Width, Height);
        image.Mutate(ctx =>
        {
            ctx.Fill(Color.FromRgb(245, 245, 245));

            for (int i = 0, length = text.Length, unit = Width / (length + 2); i < length; i++)
            {
                var x = unit * (i + 1);
                var y = Height / 2 - 20;

                var color = GetColor(rnd);
                var fontName = GetFontName(rnd);
                var fontSize = GetFontSize(rnd);
                var fontStyle = GetFontStyle();
                var location = new PointF(x, y);
                ctx.DrawText(text[i].ToString(), SystemFonts.CreateFont(fontName, fontSize, fontStyle), color, location);
                
            }
            
        });

        return image;
    }
    
    public string GenerateBase64(string text)
    {
        using var image = Generate(text);
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return Convert.ToBase64String(ms.ToArray());
    }

    public string NewText()
    {
        var xeger = new Xeger("(" + Pattern + "){" + Length + "}");
        return xeger.Generate();
    }
}