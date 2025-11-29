using System.IO;
using System.IO.Compression;

namespace Dorbit.Framework.Utils;

public static class ZipFileUtil
{
    public static MemoryStream CreateZipInMemory(string directory)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel.Optimal);

                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(file);
                fileStream.CopyTo(entryStream);
            }
        }

        // مهم!!!
        ms.Position = 0;

        return ms;
    }
}