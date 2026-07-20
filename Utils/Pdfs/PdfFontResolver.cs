#nullable enable
using System;
using System.IO;
using PdfSharp.Fonts;

namespace Dorbit.Framework.Utils.Pdfs;

public class PdfFontResolver : IFontResolver
{
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Equals("Vazir", StringComparison.OrdinalIgnoreCase))
        {
            return new FontResolverInfo($"Vazir-{(isBold ? "Bold" : "Medium")}");
        }

        return null;
    }

    public byte[]? GetFont(string faceName)
    {
        return File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", $"{faceName}.ttf"));
    }
}