using System;
using System.IO;

namespace Dorbit.Framework.Utils;

public static class DirectoryUtil
{
    public static void Copy(string source, string destination, Func<FileInfo, bool> predicate = null)
    {
        if (!File.Exists(destination)) Directory.CreateDirectory(destination);
        foreach (var filePath in Directory.GetFiles(source))
        {
            var fileInfo = new FileInfo(filePath);
            if (predicate is not null && !predicate(fileInfo)) continue;
            File.Copy(filePath, Path.Combine(destination, fileInfo.Name));
        }

        foreach (var dirPath in Directory.GetDirectories(source))
        {
            var dirInfo = new DirectoryInfo(dirPath);
            Copy(dirPath, Path.Combine(destination, dirInfo.Name));
        }
    }
}