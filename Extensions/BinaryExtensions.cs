using System.Linq;

namespace Dorbit.Framework.Extensions;

public static class BinaryExtensions
{
    public static byte[] AddChecksum(this byte[] data, byte iv = 0x5A, byte ev = 0xFF)
    {
        var checksum = iv;
        foreach (var d in data) checksum ^= d;
        checksum ^= ev;
        return data.Append(checksum).ToArray();
    }

    public static byte[] AddChecksumIf(this byte[] data, bool condition, byte iv = 0x5A, byte ev = 0xFF)
    {
        return condition ? data.AddChecksum(iv, ev) : data;
    }
}