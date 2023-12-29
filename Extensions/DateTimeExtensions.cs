namespace Dorbit.Framework.Extensions;

public static class DateTimeExtensions
{
    public static DateTime Add(this DateTime dateTime, string offset)
    {
        var lifetimeValue = Convert.ToInt32(offset.Substring(0, offset.Length - 1));

        if (offset.EndsWith("s")) dateTime = dateTime.AddSeconds(lifetimeValue);
        else if (offset.EndsWith("m")) dateTime = dateTime.AddMinutes(lifetimeValue);
        else if (offset.EndsWith("h")) dateTime = dateTime.AddHours(lifetimeValue);
        else if (offset.EndsWith("d")) dateTime = dateTime.AddDays(lifetimeValue);
        else if (offset.EndsWith("w")) dateTime = dateTime.AddDays(lifetimeValue * 7);
        else if (offset.EndsWith("M")) dateTime = dateTime.AddMonths(lifetimeValue);
        else if (offset.EndsWith("y")) dateTime = dateTime.AddYears(lifetimeValue);

        return dateTime;
    }

    public static long GetUnixTime(this DateTime dateTime)
    {
        return Math.Abs((long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
    }
}