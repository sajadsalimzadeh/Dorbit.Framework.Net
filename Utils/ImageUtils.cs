using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Dorbit.Framework.Utils;

public static class ImageUtils
{
    public static byte[] CropImage(byte[] imageBytes, Rectangle cropArea)
    {
        using var inputStream = new MemoryStream(imageBytes);
        using var image = Image.Load<Rgba32>(inputStream);

        image.Mutate(x => x.Crop(cropArea));

        using var outputStream = new MemoryStream();
        image.SaveAsPng(outputStream);
        return outputStream.ToArray();
    }

    public enum ImageCornerType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    public static byte[] CropImageCorner(byte[] imageBytes, ImageCornerType cornerType, int width, int height)
    {
        using var inputStream = new MemoryStream(imageBytes);
        using var image = Image.Load<Rgba32>(inputStream);

        var cropArea = cornerType switch
        {
            ImageCornerType.TopLeft => new Rectangle(0, 0, image.Width * width / 100, image.Height * height / 100),
            ImageCornerType.TopRight => new Rectangle(image.Width - (image.Width * width / 100), 0, (image.Width * width / 100), image.Height * height / 100),
            ImageCornerType.BottomLeft => new Rectangle(0, image.Height - (image.Height * height / 100), image.Width * width / 100, image.Height * height / 100),
            ImageCornerType.BottomRight => new Rectangle(image.Width - (image.Width * width / 100), image.Height - (image.Height * height / 100), image.Width * width / 100, image.Height * height / 100),
            _ => new Rectangle(0, 0, image.Width, image.Height)
        };

        image.Mutate(x => x.Crop(cropArea));

        using var outputStream = new MemoryStream();
        image.SaveAsPng(outputStream);
        return outputStream.ToArray();
    }
}