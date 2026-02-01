using System;
using System.Globalization;

namespace Dorbit.Framework.Extensions;

public static class DateTimeExtensions
{
    public static DateTime Add(this DateTime dateTime, string offset)
    {
        var lifetimeValue = Convert.ToInt32(offset[..^1]);

        if (offset.EndsWith("s")) dateTime = dateTime.AddSeconds(lifetimeValue);
        else if (offset.EndsWith("m")) dateTime = dateTime.AddMinutes(lifetimeValue);
        else if (offset.EndsWith("h")) dateTime = dateTime.AddHours(lifetimeValue);
        else if (offset.EndsWith("d")) dateTime = dateTime.AddDays(lifetimeValue);
        else if (offset.EndsWith("w")) dateTime = dateTime.AddDays(lifetimeValue * 7);
        else if (offset.EndsWith("M")) dateTime = dateTime.AddMonths(lifetimeValue);
        else if (offset.EndsWith("y")) dateTime = dateTime.AddYears(lifetimeValue);

        return dateTime;
    }

    public static long GetUnixTimeSeconds(this DateTime dateTime)
    {
        return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static long GetUnixTimeMilliseconds(this DateTime dateTime)
    {
        return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
    }

    public static string ToPersianDate(this DateTime dateTime, string format)
    {
        var pc = new PersianCalendar();
        return format
            .Replace("YYYY", pc.GetYear(dateTime).ToString())
            .Replace("YYY", pc.GetYear(dateTime).ToString()[1..])
            .Replace("YY", pc.GetYear(dateTime).ToString()[2..])
            .Replace("Y", pc.GetYear(dateTime).ToString()[3..])
            .Replace("MM", pc.GetMonth(dateTime).ToString().PadLeft(2, '0'))
            .Replace("M", pc.GetMonth(dateTime).ToString())
            .Replace("DD", pc.GetDayOfMonth(dateTime).ToString().PadLeft(2, '0'))
            .Replace("D", pc.GetDayOfMonth(dateTime).ToString())
            .Replace("hh", pc.GetHour(dateTime).ToString().PadLeft(2, '0'))
            .Replace("mm", pc.GetMinute(dateTime).ToString().PadLeft(2, '0'))
            .Replace("ss", pc.GetSecond(dateTime).ToString().PadLeft(2, '0'));
    }
    
    public static DateTime GetFirstDayOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}