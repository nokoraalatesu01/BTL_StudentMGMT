using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Helpers;

public static class ThuHelper
{
    private static readonly Dictionary<string, DayOfWeek> ThuToDayOfWeekMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Thứ 2", DayOfWeek.Monday },
        { "Thứ hai", DayOfWeek.Monday },
        { "Thu 2", DayOfWeek.Monday },
        { "T2", DayOfWeek.Monday },
        { "Thứ 3", DayOfWeek.Tuesday },
        { "Thứ ba", DayOfWeek.Tuesday },
        { "Thu 3", DayOfWeek.Tuesday },
        { "T3", DayOfWeek.Tuesday },
        { "Thứ 4", DayOfWeek.Wednesday },
        { "Thứ tư", DayOfWeek.Wednesday },
        { "Thu 4", DayOfWeek.Wednesday },
        { "T4", DayOfWeek.Wednesday },
        { "Thứ 5", DayOfWeek.Thursday },
        { "Thứ năm", DayOfWeek.Thursday },
        { "Thu 5", DayOfWeek.Thursday },
        { "T5", DayOfWeek.Thursday },
        { "Thứ 6", DayOfWeek.Friday },
        { "Thứ sáu", DayOfWeek.Friday },
        { "Thu 6", DayOfWeek.Friday },
        { "T6", DayOfWeek.Friday },
        { "Thứ 7", DayOfWeek.Saturday },
        { "Thứ bảy", DayOfWeek.Saturday },
        { "Thu 7", DayOfWeek.Saturday },
        { "T7", DayOfWeek.Saturday },
        { "Chủ nhật", DayOfWeek.Sunday },
        { "Chu nhat", DayOfWeek.Sunday },
        { "CN", DayOfWeek.Sunday }
    };

    public static DayOfWeek? ToDayOfWeek(string? thu)
    {
        if (string.IsNullOrWhiteSpace(thu)) return null;
        var clean = thu.Trim();
        if (ThuToDayOfWeekMap.TryGetValue(clean, out var dow))
        {
            return dow;
        }
        return null;
    }

    public static string ToThuString(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            DayOfWeek.Sunday => "Chủ nhật",
            _ => "Thứ 2"
        };
    }

    public static bool Matches(string? thu, DateTime date)
    {
        var dow = ToDayOfWeek(thu);
        return dow.HasValue && dow.Value == date.DayOfWeek;
    }

    public static bool IsSameDay(string? thu1, string? thu2)
    {
        var d1 = ToDayOfWeek(thu1);
        var d2 = ToDayOfWeek(thu2);
        return d1.HasValue && d2.HasValue && d1.Value == d2.Value;
    }
}
