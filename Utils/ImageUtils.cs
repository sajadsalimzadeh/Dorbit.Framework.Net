using System.Drawing;
using SkiaSharp;

namespace Dorbit.Framework.Utils;

public static class ImageUtils
{
    public static byte[] CropImage(byte[] imageBytes, Rectangle cropRect)
    {
        using var source = SKBitmap.Decode(imageBytes);
        using var cropped = new SKBitmap(cropRect.Width, cropRect.Height);
        using var canvas = new SKCanvas(cropped);

        var srcRect = new SKRect(cropRect.Left, cropRect.Top, cropRect.Right, cropRect.Bottom);
        var destRect = new SKRect(0, 0, cropRect.Width, cropRect.Height);
        var sampling = new SKSamplingOptions(SKFilterMode.Nearest);

        canvas.DrawBitmap(source, srcRect, destRect, sampling);

        using var image = SKImage.FromBitmap(cropped);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);

        return data.ToArray();
    }

    public enum ImageCornerType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
}