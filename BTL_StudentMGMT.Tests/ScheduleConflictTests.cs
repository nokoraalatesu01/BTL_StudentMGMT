using BTL_StudentMGMT.Helpers;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class ScheduleConflictTests
{
    [Theory]
    // 1-3 vs 3-5 trùng: [1, 3] và [3, 5] giao nhau tại tiết 3
    [InlineData("Thứ 2", 1, 3, "Thứ 2", 3, 3, true)]
    // 1-6 vs 3-4 trùng: [1, 6] bao trọn [3, 4]
    [InlineData("Thứ 2", 1, 6, "Thứ 2", 3, 2, true)]
    // 1-2 vs 3-5 không: [1, 2] và [3, 5] không giao nhau
    [InlineData("Thứ 2", 1, 2, "Thứ 2", 3, 3, false)]
    // 6-8 vs 3-5 không: [6, 8] và [3, 5] không giao nhau
    [InlineData("Thứ 2", 6, 3, "Thứ 2", 3, 3, false)]
    // 1-2 vs 3-4 (liền kề) không: tiết kết thúc là 2, lớp kia bắt đầu tiết 3 -> không trùng
    [InlineData("Thứ 2", 1, 2, "Thứ 2", 3, 2, false)]
    // Cùng đoạn trùng: 3-5 vs 3-5
    [InlineData("Thứ 2", 3, 3, "Thứ 2", 3, 3, true)]
    // Khác thứ không trùng: Thứ 2 vs Thứ 3
    [InlineData("Thứ 2", 1, 3, "Thứ 3", 1, 3, false)]
    // "Thứ 2" vs "Thứ hai" trùng: cùng ngày qua ThuHelper
    [InlineData("Thứ 2", 1, 3, "Thứ hai", 2, 3, true)]
    // Thiếu thứ hoặc SoTiet <= 0: bỏ qua (không trùng)
    [InlineData(null, 1, 3, "Thứ 2", 1, 3, false)]
    [InlineData("Thứ 2", 1, 0, "Thứ 2", 1, 3, false)]
    public void Overlaps_Theory_Tests(string? thu1, int start1, int soTiet1, string? thu2, int start2, int soTiet2, bool expected)
    {
        var result = ScheduleConflict.Overlaps(thu1, start1, soTiet1, thu2, start2, soTiet2);
        Assert.Equal(expected, result);
    }
}
