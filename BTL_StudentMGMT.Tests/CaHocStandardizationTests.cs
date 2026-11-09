using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BTL_StudentMGMT.Helpers;
using BTL_StudentMGMT.Models;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class CaHocStandardizationTests
{
    [Fact]
    public void DanhSachCa_MustHaveExactly5ShiftsWithCorrectStandards()
    {
        var caList = CaHocHelper.DanhSachCa;
        Assert.Equal(5, caList.Count);

        // Ca 1: 07:00–09:30 (tiết 1–2)
        var ca1 = caList.First(c => c.Ca == 1);
        Assert.Equal("CA1", ca1.MaCa);
        Assert.Equal("Ca 1", ca1.TenCa);
        Assert.Equal(1, ca1.TietBatDau);
        Assert.Equal(2, ca1.TietKetThuc);
        Assert.Equal("07:00", ca1.GioBatDau);
        Assert.Equal("09:30", ca1.GioKetThuc);
        Assert.Equal("07:00 – 09:30", ca1.KhungGio);

        // Ca 2: 09:35–12:00 (tiết 3–4)
        var ca2 = caList.First(c => c.Ca == 2);
        Assert.Equal("CA2", ca2.MaCa);
        Assert.Equal("Ca 2", ca2.TenCa);
        Assert.Equal(3, ca2.TietBatDau);
        Assert.Equal(4, ca2.TietKetThuc);
        Assert.Equal("09:35", ca2.GioBatDau);
        Assert.Equal("12:00", ca2.GioKetThuc);
        Assert.Equal("09:35 – 12:00", ca2.KhungGio);

        // Ca 3: 13:00–15:30 (tiết 5–6)
        var ca3 = caList.First(c => c.Ca == 3);
        Assert.Equal("CA3", ca3.MaCa);
        Assert.Equal("Ca 3", ca3.TenCa);
        Assert.Equal(5, ca3.TietBatDau);
        Assert.Equal(6, ca3.TietKetThuc);
        Assert.Equal("13:00", ca3.GioBatDau);
        Assert.Equal("15:30", ca3.GioKetThuc);
        Assert.Equal("13:00 – 15:30", ca3.KhungGio);

        // Ca 4: 15:35–18:00 (tiết 7–8)
        var ca4 = caList.First(c => c.Ca == 4);
        Assert.Equal("CA4", ca4.MaCa);
        Assert.Equal("Ca 4", ca4.TenCa);
        Assert.Equal(7, ca4.TietBatDau);
        Assert.Equal(8, ca4.TietKetThuc);
        Assert.Equal("15:35", ca4.GioBatDau);
        Assert.Equal("18:00", ca4.GioKetThuc);
        Assert.Equal("15:35 – 18:00", ca4.KhungGio);

        // Ca 5: 18:15–20:45 (tiết 9–10)
        var ca5 = caList.First(c => c.Ca == 5);
        Assert.Equal("CA5", ca5.MaCa);
        Assert.Equal("Ca 5", ca5.TenCa);
        Assert.Equal(9, ca5.TietBatDau);
        Assert.Equal(10, ca5.TietKetThuc);
        Assert.Equal("18:15", ca5.GioBatDau);
        Assert.Equal("20:45", ca5.GioKetThuc);
        Assert.Equal("18:15 – 20:45", ca5.KhungGio);
    }

    [Fact]
    public void GioCacTiet_MustHavePeriods1To10AndExclude11And12()
    {
        var gioTiet = CaHocHelper.GioCacTiet;
        Assert.Equal(10, gioTiet.Count);
        Assert.False(gioTiet.ContainsKey(11));
        Assert.False(gioTiet.ContainsKey(12));

        Assert.Equal(("07:00", "08:10"), gioTiet[1]);
        Assert.Equal(("08:20", "09:30"), gioTiet[2]);
        Assert.Equal(("09:35", "10:45"), gioTiet[3]);
        Assert.Equal(("10:50", "12:00"), gioTiet[4]);
        Assert.Equal(("13:00", "14:10"), gioTiet[5]);
        Assert.Equal(("14:20", "15:30"), gioTiet[6]);
        Assert.Equal(("15:35", "16:45"), gioTiet[7]);
        Assert.Equal(("16:50", "18:00"), gioTiet[8]);
        Assert.Equal(("18:15", "19:25"), gioTiet[9]);
        Assert.Equal(("19:35", "20:45"), gioTiet[10]);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(6, 3)]
    [InlineData(7, 4)]
    [InlineData(8, 4)]
    [InlineData(9, 5)]
    [InlineData(10, 5)]
    [InlineData(0, 1)]   // fallback
    [InlineData(11, 5)]  // fallback
    public void GetCaTuTiet_ShouldReturnCorrectCa(int tiet, int expectedCa)
    {
        var ca = CaHocHelper.GetCaTuTiet(tiet);
        Assert.Equal(expectedCa, ca);
    }

    [Theory]
    [InlineData(1, "Ca 1")]
    [InlineData(2, "Ca 2")]
    [InlineData(3, "Ca 3")]
    [InlineData(4, "Ca 4")]
    [InlineData(5, "Ca 5")]
    public void GetTenCa_ShouldReturnCa1ToCa5(int ca, string expectedName)
    {
        var ten = CaHocHelper.GetTenCa(ca);
        Assert.Equal(expectedName, ten);
    }

    [Theory]
    [InlineData(1, 2, "07:00 – 09:30")]
    [InlineData(3, 2, "09:35 – 12:00")]
    [InlineData(5, 2, "13:00 – 15:30")]
    [InlineData(7, 2, "15:35 – 18:00")]
    [InlineData(9, 2, "18:15 – 20:45")]
    [InlineData(3, 4, "09:35 – 15:30")] // Nhiều ca (tiết 3–6)
    public void GetKhungGio_ShouldReturnCorrectFrame(int tietBatDau, int soTiet, string expectedKhungGio)
    {
        var khungGio = CaHocHelper.GetKhungGio(tietBatDau, soTiet);
        Assert.Equal(expectedKhungGio, khungGio);
    }

    [Fact]
    public void CalculateTimelinePosition_Ca5_ShouldNotClipAndUseCorrectEndTime()
    {
        var (top, height, startStr, endStr) = CaHocHelper.CalculateTimelinePosition(9, 2);
        Assert.Equal("18:15", startStr);
        Assert.Equal("20:45", endStr);
        // 18:15 is 675 min from 7:00 -> 675 * 50.0 / 60.0 = 562.5 (MidpointRounding.ToEven -> 562)
        Assert.Equal((int)Math.Round(675 * 50.0 / 60.0), top);
        // duration 150 min -> 150 * 50 / 60 = 125px -> min height is 140px
        Assert.Equal(140, height);
        // top + height = 702px, which comfortably fits in a 720px grid
        Assert.True(top + height <= 720);
    }

    [Fact]
    public void GetCacCaGiaoNhau_SpanningClass_ReturnsAllCoveredShifts()
    {
        // Tiết 3-6: Ca 2 (tiết 3-4) và Ca 3 (tiết 5-6)
        var cacCa = CaHocHelper.GetCacCaGiaoNhau(3, 6);
        Assert.Equal(new List<int> { 2, 3 }, cacCa);

        // Tiết 9-10: Ca 5
        var ca5 = CaHocHelper.GetCacCaGiaoNhau(9, 10);
        Assert.Equal(new List<int> { 5 }, ca5);
    }

    [Theory]
    [InlineData("Thứ 2", 9, 2, "Thứ 2", 9, 2, true)]   // Cả 2 đều học ca 5 tiết 9-10 -> trùng
    [InlineData("Thứ 2", 7, 2, "Thứ 2", 9, 2, false)]  // Ca 4 (tiết 7-8) vs Ca 5 (tiết 9-10) -> không trùng
    [InlineData("Thứ 2", 8, 3, "Thứ 2", 9, 2, true)]   // Tiết 8-10 vs Tiết 9-10 -> trùng
    [InlineData("Thứ 3", 9, 2, "Thứ 2", 9, 2, false)]  // Khác thứ -> không trùng
    public void ScheduleConflict_Ca5_OverlapsCorrectly(string thu1, int start1, int count1, string thu2, int start2, int count2, bool expected)
    {
        var result = ScheduleConflict.Overlaps(thu1, start1, count1, thu2, start2, count2);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void LopHocPhan_RangeValidation_ShouldRejectTietBatDauGreaterThan10()
    {
        var model = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT101.01",
            TenLopHocPhan = "Lập trình C++",
            MonHocId = 1,
            TietBatDau = 11, // > 10
            SoTiet = 2
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(model.TietBatDau)));
    }
}
