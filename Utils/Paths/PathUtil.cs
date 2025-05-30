using System.IO;

namespace Dorbit.Framework.Utils.Paths;

public static class PathUtil
{
    public static string Find(string filename, string dir)
    {
        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(filename))
        {
            return string.Empty;
        }

        if (!Directory.Exists(dir))
        {
            return string.Empty;
        }

        var files = Directory.GetFiles(dir, filename, SearchOption.AllDirectories);
        return files.Length < 1 ? null : files[0];
    }

    public static string ToFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var separators = new[] { '/', '\\' };
        var sp = path.Split(separators);
        return sp[^1];
    }

    public static string RemoveExtension(string fileName)
    {
        return fileName.Replace(".xml", string.Empty).Replace(".XML", string.Empty);
    }
}