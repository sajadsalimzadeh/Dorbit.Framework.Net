#pragma warning disable CA1416 // Validate platform compatibility
using Fare;
using Devor.Framework.Enums;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace Devor.Framework.Utils.Captcha
{
    public class CaptchaGenerator
    {
        public CaptchaDificulty Dificulty { get; set; } = CaptchaDificulty.Normal;
        public int Width { get; set; } = 80;
        public int Height { get; set; } = 200;
        public int Length { get; set; } = 6;
        public string Pattern { get; set; } = "[0-9A-Z]";
        public HatchStyle[] HatchStyles { get => HatchStyles1; set => HatchStyles1 = value; }
        public HatchStyle[] HatchStyles1 { get => hatchStyles; set => hatchStyles = value; }

        int[] backgroundNoiseColors = new int[] { 150, 150, 150 };
        int[] textColors = new int[] { 0, 0, 0 };

        string[] fontNames = new string[]
        {
                "Comic Sans MS",
                "Arial",
                "Times New Roman",
                "Georgia",
                "Verdana",
                "Geneva"
        };

        FontStyle[] fontStyles = new FontStyle[]
        {
                FontStyle.Regular,
                FontStyle.Bold,
                FontStyle.Italic,
                FontStyle.Underline,
                FontStyle.Strikeout,
        };

        HatchStyle[] hatchStyles = new HatchStyle[]
        {
                HatchStyle.BackwardDiagonal, HatchStyle.Cross,
                HatchStyle.DashedDownwardDiagonal, HatchStyle.DashedHorizontal,
                HatchStyle.DashedUpwardDiagonal, HatchStyle.DashedVertical,
                HatchStyle.DiagonalBrick, HatchStyle.DiagonalCross,
                HatchStyle.Divot, HatchStyle.DottedDiamond, HatchStyle.DottedGrid,
                HatchStyle.ForwardDiagonal, HatchStyle.Horizontal,
                HatchStyle.HorizontalBrick, HatchStyle.LargeCheckerBoard,
                HatchStyle.LargeConfetti, HatchStyle.LargeGrid,
                HatchStyle.LightDownwardDiagonal, HatchStyle.LightHorizontal,
                HatchStyle.LightUpwardDiagonal, HatchStyle.LightVertical,
                HatchStyle.Max, HatchStyle.Min, HatchStyle.NarrowHorizontal,
                HatchStyle.NarrowVertical, HatchStyle.OutlinedDiamond,
                HatchStyle.Plaid, HatchStyle.Shingle, HatchStyle.SmallCheckerBoard,
                HatchStyle.SmallConfetti, HatchStyle.SmallGrid,
                HatchStyle.SolidDiamond, HatchStyle.Sphere, HatchStyle.Trellis,
                HatchStyle.Vertical, HatchStyle.Wave, HatchStyle.Weave,
                HatchStyle.WideDownwardDiagonal, HatchStyle.WideUpwardDiagonal, HatchStyle.ZigZag
        };

        private int GetRotation(System.Random rnd)
        {
            return Dificulty switch
            {
                CaptchaDificulty.VeryEasy => 0,
                CaptchaDificulty.Easy => rnd.Next(-10, 10),
                CaptchaDificulty.Normal => rnd.Next(-30, 30),
                CaptchaDificulty.Hard => rnd.Next(-50, 50),
                _ => rnd.Next(-60, 60),
            };
        }

        private int GetFontSize(System.Random rnd)
        {
            return Dificulty switch
            {
                CaptchaDificulty.VeryEasy => 20,
                CaptchaDificulty.Easy => rnd.Next(20, 25),
                CaptchaDificulty.Normal => rnd.Next(15, 25),
                CaptchaDificulty.Hard => rnd.Next(10, 30),
                _ => rnd.Next(10, 35),
            };
        }

        private FontStyle GetFontStyle(System.Random rnd)
        {
            return Dificulty switch
            {
                CaptchaDificulty.VeryEasy => FontStyle.Regular,
                CaptchaDificulty.Easy => fontStyles[rnd.Next(0, 1)],
                CaptchaDificulty.Normal => fontStyles[rnd.Next(0, 2)],
                CaptchaDificulty.Hard => fontStyles[rnd.Next(0, 3)],
                _ => fontStyles[rnd.Next(0, 4)],
            };
        }

        private string GetFontName(System.Random rnd)
        {
            switch (Dificulty)
            {
                case CaptchaDificulty.VeryEasy: return fontNames[0];
                case CaptchaDificulty.Easy: return fontNames[rnd.Next(0, 1)];
                case CaptchaDificulty.Normal: return fontNames[rnd.Next(0, 2)];
                case CaptchaDificulty.Hard: return fontNames[rnd.Next(0, 4)];
                case CaptchaDificulty.VeryHard:
                default:
                    return fontNames[rnd.Next(0, 5)];
            }
        }

        private int[] GetColor(System.Random rnd)
        {
            switch (Dificulty)
            {
                case CaptchaDificulty.VeryEasy: return new int[] { 0, 0, 0 };
                case CaptchaDificulty.Easy: return new int[] { rnd.Next(0, 30), rnd.Next(0, 30), rnd.Next(0, 30) };
                case CaptchaDificulty.Normal: return new int[] { rnd.Next(0, 60), rnd.Next(0, 60), rnd.Next(0, 60) };
                case CaptchaDificulty.Hard: return new int[] { rnd.Next(0, 90), rnd.Next(0, 90), rnd.Next(0, 90) };
                case CaptchaDificulty.VeryHard:
                default:
                    return new int[] { rnd.Next(0, 150), rnd.Next(0, 150), rnd.Next(0, 150) };
            }
        }

        public Image Generate(string text)
        {
            System.Random rnd = new System.Random();

            //Creates an output Bitmap
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            var graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            //Create a Drawing area
            var rectangle = new RectangleF(0, 0, Width, Height);

            for (int i = 0; i <= (int)Dificulty; i++)
            {
                //Draw background (Lighter colors RGB 100 to 255)
                Brush brush = new SolidBrush(Color.FromArgb(245, 245, 245));

                graphics.FillRectangle(brush, rectangle);
            }

            var matrix = new Matrix();
            for (int i = 0, length = text.Length, unit = Width / (length + 2); i < length; i++)
            {
                matrix.Reset();
                int x = unit * (i + 1);
                int y = Height / 2 - 20;

                //Rotate text Random
                matrix.RotateAt(GetRotation(rnd), new PointF(x, y));
                graphics.Transform = matrix;

                //Draw the letters with Random Font Type, Size and Color
                var color = GetColor(rnd);
                graphics.DrawString
                (
                    //Text
                    text.Substring(i, 1),
                    //Random Font Name and Style
                    new Font(GetFontName(rnd), GetFontSize(rnd), GetFontStyle(rnd)),
                    //Random Color (Darker colors RGB 0 to 100)
                    new SolidBrush(Color.FromArgb(118, 38, 133)),
                    x,
                    y//rnd.Next(10, 40)
                );
                graphics.ResetTransform();
            }
            return bitmap;
        }

        public string GenerateBase64(string text)
        {
            using Image image = Generate(text);
            using MemoryStream memory = new MemoryStream();
            image.Save(memory, ImageFormat.Png);
            byte[] imageBytes = memory.ToArray();
            // Convert byte[] to Base64 String
            return Convert.ToBase64String(imageBytes);
        }

        public string NewText()
        {
            var xeger = new Xeger("(" + Pattern + "){" + Length + "}");
            return xeger.Generate();
        }
    }
}
