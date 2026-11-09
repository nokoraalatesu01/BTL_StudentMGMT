using System;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Helpers;

public static class ScheduleConflict
{
    /// <summary>
    /// Kiểm tra trùng lịch giữa 2 khung giờ học:
    /// - Đoạn tiết: [start, start + SoTiet - 1]
    /// - Trùng khi cùng thứ && newStart <= otherEnd && newEnd >= otherStart.
    /// - Bỏ qua (trả về false) khi thiếu Thu hoặc SoTiet <= 0.
    /// - So sánh thứ qua ThuHelper.
    /// </summary>
    public static bool Overlaps(string? thu1, int start1, int soTiet1, string? thu2, int start2, int soTiet2)
    {
        if (string.IsNullOrWhiteSpace(thu1) || string.IsNullOrWhiteSpace(thu2))
            return false;

        if (soTiet1 <= 0 || soTiet2 <= 0)
            return false;

        if (!ThuHelper.IsSameDay(thu1, thu2))
            return false;

        int newStart = start1;
        int newEnd = start1 + soTiet1 - 1;

        int otherStart = start2;
        int otherEnd = start2 + soTiet2 - 1;

        return newStart <= otherEnd && newEnd >= otherStart;
    }

    /// <summary>
    /// Kiểm tra trùng lịch giữa 2 lớp học phần
    /// </summary>
    public static bool Overlaps(LopHocPhan? a, LopHocPhan? b)
    {
        if (a == null || b == null)
            return false;

        return Overlaps(a.Thu, a.TietBatDau, a.SoTiet, b.Thu, b.TietBatDau, b.SoTiet);
    }
}
