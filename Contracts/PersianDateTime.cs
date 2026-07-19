using System;
using System.Globalization;
using Dorbit.Framework.Extensions;

namespace Dorbit.Framework.Contracts;

public class PersianDateTime
{
    private static PersianCalendar pc = new();
    public DateTime DateTime { get; private set; }

    public PersianDateTime(DateTime dateTime)
    {
        var utcDate = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
        DateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, tehranTimeZone);
    }

    public PersianDateTime AddDays(int value)
    {
        return new PersianDateTime(DateTime.AddDays(value));
    }

    public PersianDateTime AddHours(int value)
    {
        return new PersianDateTime(DateTime.AddHours(value));
    }

    public PersianDateTime AddMinutes(int value)
    {
        return new PersianDateTime(DateTime.AddMinutes(value));
    }

    public PersianDateTime AddSeconds(int value)
    {
        return new PersianDateTime(DateTime.AddSeconds(value));
    }

    public int GetYear()
    {
        return pc.GetYear(DateTime);
    }

    public int GetMonth()
    {
        return pc.GetMonth(DateTime);
    }

    public int GetWeekOfMonth()
    {
        return pc.GetWeekOfMonth(DateTime);
    }

    public int GetDayOfMonth()
    {
        return pc.GetDayOfMonth(DateTime);
    }

    public int GetDayOfWeek()
    {
        return pc.GetDayOfMonth(DateTime) % 7;
    }

    public int GetHour()
    {
        return pc.GetHour(DateTime);
    }

    public int GetMinute()
    {
        return pc.GetMinute(DateTime);
    }

    public int GetSecond()
    {
        return pc.GetSecond(DateTime);
    }

    public string Format(string format)
    {
        var year = GetYear().ToString();
        var month = GetMonth().ToString();
        var weekOfMonth = GetWeekOfMonth().ToString();
        var dayOfMonth = GetDayOfMonth().ToString();

        return format
            .Replace("YYYY", year)
            .Replace("YYY", year[1..])
            .Replace("YY", year[2..])
            .Replace("Y", year[3..])
            .Replace("MM", month.PadLeft(2, '0'))
            .Replace("M", month)
            .Replace("WW", weekOfMonth.PadLeft(2, '0'))
            .Replace("W", weekOfMonth)
            .Replace("DD", dayOfMonth.PadLeft(2, '0'))
            .Replace("D", dayOfMonth)
            .Replace("hh", GetHour().ToString().PadLeft(2, '0'))
            .Replace("mm", GetMinute().ToString().PadLeft(2, '0'))
            .Replace("ss", GetSecond().ToString().PadLeft(2, '0'));
    }
}