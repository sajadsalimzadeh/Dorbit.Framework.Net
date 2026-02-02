namespace Dorbit.Framework.Extensions;

public static class NumberExtensions
{
    public static string ToHex(this int number)
    {
        var hex = number.ToString("X");
        return hex.Length % 2 == 0 ? hex : "0" + hex;
    }
}