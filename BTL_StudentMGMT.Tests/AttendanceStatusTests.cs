using BTL_StudentMGMT.Models;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class AttendanceStatusTests
{
    [Theory]
    [InlineData("PRESENT", true)]
    [InlineData("ABSENT", true)]
    [InlineData("EXCUSED", true)]
    [InlineData("LATE", true)]
    [InlineData("Có mặt", true)]
    [InlineData("Vắng", true)]
    [InlineData("Có phép", true)]
    [InlineData("Đi muộn", true)]
    [InlineData("UNKNOWN", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void IsValid_ShouldReturnExpectedResult(string? status, bool expected)
    {
        var result = AttendanceStatus.IsValid(status);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Có mặt", AttendanceStatus.Present)]
    [InlineData("Co mat", AttendanceStatus.Present)]
    [InlineData("CoMat", AttendanceStatus.Present)]
    [InlineData("PRESENT", AttendanceStatus.Present)]
    [InlineData("present", AttendanceStatus.Present)]
    [InlineData("Vắng", AttendanceStatus.Absent)]
    [InlineData("Vang", AttendanceStatus.Absent)]
    [InlineData("ABSENT", AttendanceStatus.Absent)]
    [InlineData("Có phép", AttendanceStatus.Excused)]
    [InlineData("Co phep", AttendanceStatus.Excused)]
    [InlineData("EXCUSED", AttendanceStatus.Excused)]
    [InlineData("Đi muộn", AttendanceStatus.Late)]
    [InlineData("LATE", AttendanceStatus.Late)]
    public void Normalize_ShouldMapToStandardEnglishCode(string input, string expected)
    {
        var result = AttendanceStatus.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(AttendanceStatus.Present, "Có mặt")]
    [InlineData(AttendanceStatus.Absent, "Vắng")]
    [InlineData(AttendanceStatus.Excused, "Có phép")]
    [InlineData(AttendanceStatus.Late, "Đi muộn")]
    [InlineData("UNKNOWN_CODE", "UNKNOWN_CODE")]
    [InlineData(null, "Chưa điểm danh")]
    public void ToDisplayName_ShouldReturnExpectedVietnameseText(string? input, string expected)
    {
        var result = AttendanceStatus.ToDisplayName(input);
        Assert.Equal(expected, result);
    }
}
