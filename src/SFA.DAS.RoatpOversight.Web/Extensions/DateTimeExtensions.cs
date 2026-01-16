using System;

namespace SFA.DAS.RoatpOversight.Web.Extensions;

public static class DateTimeExtensions
{
    public static string ToSfaShortDateString(this DateTime time)
    {
        return time.ToString("dd MMMM yyyy");
    }

    public static string ToSfaShortDateString(this DateTime? time)
    {
        return time?.ToString("dd MMMM yyyy");
    }
}
