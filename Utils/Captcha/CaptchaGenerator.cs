using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Dorbit.Framework.Contracts;

namespace Dorbit.Framework.Utils.Captcha;

public class CaptchaGenerator
{
    public CaptchaDificulty Difficulty { get; set; } = CaptchaDificulty.Normal;
    public int Width { get; set; } = 80;
    public int Height { get; set; } = 200;

    private static readonly List<SKTypeface> Typefaces = [];

    static CaptchaGenerator()
    {
        Typefaces.Add(SKTypeface.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "Vazir-Bold.ttf")));
        Typefaces.Add(SKTypeface.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "Vazir-Medium.ttf")));
        Typefaces.Add(SKTypeface.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "Vazir-Regular.ttf")));
    }

    private int GetRotation()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => 0,
            CaptchaDificulty.Easy => RandomNumberGenerator.GetInt32(-10, 10),
            CaptchaDificulty.Normal => RandomNumberGenerator.GetInt32(-30, 30),
            CaptchaDificulty.Hard => RandomNumberGenerator.GetInt32(-50, 50),
            _ => RandomNumberGenerator.GetInt32(-60, 60),
        };
    }

    private int GetFontSize()
    {
        return Difficulty switch
        {
            CaptchaDificulty.VeryEasy => RandomNumberGenerator.GetInt32((int)(Height * .7), (int)(Height * .9)),
            CaptchaDificulty.Easy => RandomNumberGenerator.GetInt32((int)(Height * .6), (int)(Height * .8)),
            CaptchaDificulty.Normal => RandomNumberGenerator.GetInt32((int)(Height * .5), (int)(Height * .7)),
            CaptchaDificulty.Hard => RandomNumberGenerator.GetInt32((int)(Height * .4), (int)(Height * .6)),
            _ => RandomNumberGenerator.GetInt32((int)(Height * .3), (int)(Height * .5)),
        };
    }

    private SKTypeface GetTypeface()
    {
        switch (Difficulty)
        {
            case CaptchaDificulty.VeryEasy: return Typefaces[0];
            case CaptchaDificulty.Easy: return Typefaces[RandomNumberGenerator.GetInt32(0, Math.Min(1, Typefaces.Count - 1))];
            case CaptchaDificulty.Normal: return Typefaces[RandomNumberGenerator.GetInt32(0, Math.Min(2, Typefaces.Count - 1))];
            case CaptchaDificulty.Hard: return Typefaces[RandomNumberGenerator.GetInt32(0, Math.Min(4, Typefaces.Count - 1))];
            case CaptchaDificulty.VeryHard:
            case CaptchaDificulty.None:
            default:
                return Typefaces[RandomNumberGenerator.GetInt32(0, Typefaces.Count)];
        }
    }

    private SKColor GetColor()
    {
        return new SKColor((byte)RandomNumberGenerator.GetInt32(0, 200), (byte)RandomNumberGenerator.GetInt32(0, 200), (byte)RandomNumberGenerator.GetInt32(0, 200));
    }

    private void DrawNoiseLines(SKCanvas canvas, int width, int height)
    {
        for (int i = 0; i < 4; i++)
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.StrokeWidth = RandomNumberGenerator.GetInt32(1, 3);
            paint.Color = GetColor();

            float x1 = RandomNumberGenerator.GetInt32(width);
            float y1 = RandomNumberGenerator.GetInt32(height);

            float x2 = RandomNumberGenerator.GetInt32(width);
            float y2 = RandomNumberGenerator.GetInt32(height);

            canvas.DrawLine(x1, y1, x2, y2, paint);
        }
    }

    private void DrawNoiseDots(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;

        for (int i = 0; i < 60; i++)
        {
            paint.Color = GetColor();

            float x = RandomNumberGenerator.GetInt32(width);
            float y = RandomNumberGenerator.GetInt32(height);

            float radius = RandomNumberGenerator.GetInt32(1, 3);

            canvas.DrawCircle(x, y, radius, paint);
        }
    }

    public byte[] Generate(string text)
    {
        var typeface = GetTypeface();
        var info = new SKImageInfo(Width, Height);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        DrawNoiseLines(canvas, Width, Height);
        DrawNoiseDots(canvas, Width, Height);
        
        for (int i = 0, length = text.Length, unit = Width / (length + 2); i < length; i++)
        {
            var x = unit * (i + 1);
            var y = Height / 2 + 20;

            var fontSize = GetFontSize();

            using var font = new SKFont(typeface);
            font.Size = fontSize;
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = GetColor();

            canvas.Save();
            canvas.RotateDegrees(GetRotation(), x, y);

            canvas.DrawText(
                text[i].ToString(),
                x,
                y,
                SKTextAlign.Center,
                font,
                paint
            );

            canvas.Restore();
        }


        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public string GenerateBase64(string text)
    {
        var image = Generate(text);
        return Convert.ToBase64String(image);
    }
}