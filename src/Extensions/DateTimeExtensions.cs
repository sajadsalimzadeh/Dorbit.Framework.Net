using System;
using System.Globalization;

namespace Devor.Framework.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToPersianDate(this DateTime dateTime, string format = "YYYY/MM/DD hh:mm:ss")
        {
            var pc = new PersianCalendar();
            var year = pc.GetYear(dateTime);
            var month = pc.GetMonth(dateTime);
            var day = pc.GetDayOfMonth(dateTime);
            var hour = pc.GetHour(dateTime);
            var min = pc.GetMinute(dateTime);
            var sec = pc.GetSecond(dateTime);

            format = format.Replace("YYYY", year.ToString());
            format = format.Replace("MM", (month < 10 ? "0" : "") + month.ToString());
            format = format.Replace("DD", (day < 10 ? "0" : "") + day.ToString());
            format = format.Replace("hh", (hour < 10 ? "0" : "") + hour.ToString());
            format = format.Replace("mm", (min < 10 ? "0" : "") + min.ToString());
            format = format.Replace("ss", (sec < 10 ? "0" : "") + sec.ToString());
            return format;
        }
    }
}
