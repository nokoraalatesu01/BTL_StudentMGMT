using System;
using BTL_StudentMGMT.Helpers;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class ThuHelperTests
{
    [Theory]
    [InlineData("Thứ 2", DayOfWeek.Monday)]
    [InlineData("Thứ hai", DayOfWeek.Monday)]
    [InlineData("T2", DayOfWeek.Monday)]
    [InlineData("Thứ 3", DayOfWeek.Tuesday)]
    [InlineData("Thứ 4", DayOfWeek.Wednesday)]
    [InlineData("Thứ 5", DayOfWeek.Thursday)]
    [InlineData("Thứ 6", DayOfWeek.Friday)]
    [InlineData("Thứ 7", DayOfWeek.Saturday)]
    [InlineData("Chủ nhật", DayOfWeek.Sunday)]
    [InlineData("CN", DayOfWeek.Sunday)]
    public void ToDayOfWeek_ShouldMapCorrectly(string thu, DayOfWeek expected)
    {
        var result = ThuHelper.ToDayOfWeek(thu);
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void ToDayOfWeek_ShouldReturnNullForInvalidString()
    {
        Assert.Null(ThuHelper.ToDayOfWeek("Thứ 8"));
        Assert.Null(ThuHelper.ToDayOfWeek(""));
        Assert.Null(ThuHelper.ToDayOfWeek("   "));
        Assert.Null(ThuHelper.ToDayOfWeek(null));
    }

    [Fact]
    public void Matches_ShouldCorrectlyMatchDate()
    {
        // 2026-11-02 is Monday
        var monday = new DateTime(2026, 11, 2);
        Assert.True(ThuHelper.Matches("Thứ 2", monday));
        Assert.False(ThuHelper.Matches("Thứ 3", monday));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, "Thứ 2")]
    [InlineData(DayOfWeek.Tuesday, "Thứ 3")]
    [InlineData(DayOfWeek.Wednesday, "Thứ 4")]
    [InlineData(DayOfWeek.Thursday, "Thứ 5")]
    [InlineData(DayOfWeek.Friday, "Thứ 6")]
    [InlineData(DayOfWeek.Saturday, "Thứ 7")]
    [InlineData(DayOfWeek.Sunday, "Chủ nhật")]
    public void ToThuString_ShouldFormatDayOfWeek(DayOfWeek dow, string expected)
    {
        var result = ThuHelper.ToThuString(dow);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Thứ 2", "Thứ hai")]
    [InlineData("T2", "Thu 2")]
    [InlineData("Chủ nhật", "CN")]
    public void IsSameDay_SameDayAliases_ReturnsTrue(string thu1, string thu2)
    {
        Assert.True(ThuHelper.IsSameDay(thu1, thu2));
    }

    [Theory]
    [InlineData("Thứ 2", "Thứ 3")]
    [InlineData("Thứ 6", "Thứ 7")]
    public void IsSameDay_DifferentDays_ReturnsFalse(string thu1, string thu2)
    {
        Assert.False(ThuHelper.IsSameDay(thu1, thu2));
    }

    [Theory]
    [InlineData(null, "Thứ 2")]
    [InlineData("Thứ 2", null)]
    [InlineData("invalid", "Thứ 2")]
    public void IsSameDay_NullOrInvalid_ReturnsFalse(string? thu1, string? thu2)
    {
        Assert.False(ThuHelper.IsSameDay(thu1, thu2));
    }
}
