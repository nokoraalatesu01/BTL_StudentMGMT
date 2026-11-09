using System.Collections.Generic;
using BTL_StudentMGMT.Models;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class AttendanceCalculatorTests
{
    private readonly AttendanceOptions _defaultOptions = new AttendanceOptions
    {
        AbsenceThresholdPercent = 20,
        EditWindowDays = 7,
        CountExcusedAsAbsent = false,
        MinSessionsForWarning = 3
    };

    [Fact]
    public void TongBuoi_WhenZero_ShouldReturnBinhThuongAndNoSuggestion()
    {
        var result = AttendanceCalculator.Calculate(present: 0, late: 0, excused: 0, absent: 0, tongBuoi: 0, _defaultOptions);

        Assert.Equal(0, result.TongBuoi);
        Assert.Equal(0.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.BinhThuong, result.MucCanhBao);
        Assert.Null(result.DiemGoiY);
        Assert.Null(result.DienGiaiGoiY);
        Assert.Equal("Bình thường", result.TenMucCanhBao);
    }

    [Fact]
    public void MinSessionsForWarning_WhenTongBuoiLessThanMin_ShouldReturnBinhThuongEvenIfAllAbsent()
    {
        // TongBuoi = 2 < MinSessionsForWarning (3)
        // Dù vắng 2/2 = 100%, vẫn là BinhThuong
        var result = AttendanceCalculator.Calculate(present: 0, late: 0, excused: 0, absent: 2, tongBuoi: 2, _defaultOptions);

        Assert.Equal(2, result.TongBuoi);
        Assert.Equal(100.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.BinhThuong, result.MucCanhBao);
    }

    [Fact]
    public void AbsenceThreshold_Exactly20Percent_ShouldReturnNguyCo()
    {
        // 10 buổi, vắng đúng 2 buổi = 20.0% -> NguyCo
        var result = AttendanceCalculator.Calculate(present: 8, late: 0, excused: 0, absent: 2, tongBuoi: 10, _defaultOptions);

        Assert.Equal(10, result.TongBuoi);
        Assert.Equal(20.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.NguyCo, result.MucCanhBao);
        Assert.Equal("Nguy cơ cấm thi", result.TenMucCanhBao);
    }

    [Fact]
    public void AbsenceThreshold_Warning70Percent_ShouldReturnCanhBao()
    {
        // Ngưỡng cảnh báo: 70% của 20% = 14%
        // 20 buổi, vắng 3 buổi = 15.0% (>= 14% và < 20%) -> CanhBao
        var result = AttendanceCalculator.Calculate(present: 17, late: 0, excused: 0, absent: 3, tongBuoi: 20, _defaultOptions);

        Assert.Equal(20, result.TongBuoi);
        Assert.Equal(15.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.CanhBao, result.MucCanhBao);
        Assert.Equal("Cảnh báo vắng", result.TenMucCanhBao);
    }

    [Fact]
    public void AbsenceThreshold_Below14Percent_ShouldReturnBinhThuong()
    {
        // 20 buổi, vắng 2 buổi = 10.0% (< 14%) -> BinhThuong
        var result = AttendanceCalculator.Calculate(present: 18, late: 0, excused: 0, absent: 2, tongBuoi: 20, _defaultOptions);

        Assert.Equal(20, result.TongBuoi);
        Assert.Equal(10.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.BinhThuong, result.MucCanhBao);
        Assert.Equal("Bình thường", result.TenMucCanhBao);
    }

    [Fact]
    public void CountExcusedAsAbsent_WhenFalse_ShouldNotCountExcusedAsAbsent()
    {
        var options = new AttendanceOptions
        {
            AbsenceThresholdPercent = 20,
            CountExcusedAsAbsent = false,
            MinSessionsForWarning = 3
        };

        // 10 buổi: 7 CM, 2 CP, 1 V
        // Vắng = 1 buổi -> % vắng = 10.0% (BinhThuong)
        var result = AttendanceCalculator.Calculate(present: 7, late: 0, excused: 2, absent: 1, tongBuoi: 10, options);

        Assert.Equal(10.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.BinhThuong, result.MucCanhBao);
        // Có phép vẫn tính là có mặt khi CountExcusedAsAbsent = false -> (7 + 2) / 10 * 10 = 9.0
        Assert.Equal(9.0, result.DiemGoiY);
    }

    [Fact]
    public void CountExcusedAsAbsent_WhenTrue_ShouldCountExcusedAsAbsent()
    {
        var options = new AttendanceOptions
        {
            AbsenceThresholdPercent = 20,
            CountExcusedAsAbsent = true,
            MinSessionsForWarning = 3
        };

        // 10 buổi: 7 CM, 2 CP, 1 V
        // Vắng = 1 + 2 = 3 buổi -> % vắng = 30.0% (NguyCo)
        var result = AttendanceCalculator.Calculate(present: 7, late: 0, excused: 2, absent: 1, tongBuoi: 10, options);

        Assert.Equal(30.0, result.PhanTramVang);
        Assert.Equal(AttendanceWarningLevel.NguyCo, result.MucCanhBao);
        // Có phép KHÔNG được cộng điểm khi CountExcusedAsAbsent = true -> 7 / 10 * 10 = 7.0
        Assert.Equal(7.0, result.DiemGoiY);
    }

    [Fact]
    public void Late_CountedAsPresent_ShouldAffectScoreSuggestion()
    {
        // 10 buổi: 6 CM, 2 DM (Đi muộn), 1 CP, 1 V
        // Đi muộn được tính là có mặt
        // Số buổi có mặt = 6 (CM) + 2 (DM) + 1 (CP) = 9
        // Điểm gợi ý = round(10 * 9 / 10, 1) = 9.0
        var result = AttendanceCalculator.Calculate(present: 6, late: 2, excused: 1, absent: 1, tongBuoi: 10, _defaultOptions);

        Assert.Equal(10, result.TongBuoi);
        Assert.Equal(9, result.SoBuoiCoMat);
        Assert.Equal(9.0, result.DiemGoiY);
        Assert.Equal("dựa trên 9/10 buổi", result.DienGiaiGoiY);
        Assert.Equal(10.0, result.PhanTramVang); // Vắng = 1/10 = 10%
    }

    [Fact]
    public void GradeSuggestion_Formula_RoundsToOneDecimalPlace()
    {
        // 15 buổi: 11 CM, 1 DM, 0 CP, 3 V
        // Số buổi có mặt = 12 / 15 -> 10 * 12 / 15 = 8.0
        var result1 = AttendanceCalculator.Calculate(present: 11, late: 1, excused: 0, absent: 3, tongBuoi: 15, _defaultOptions);
        Assert.Equal(8.0, result1.DiemGoiY);

        // 14 buổi: 11 CM, 0 DM, 0 CP, 3 V
        // Số buổi có mặt = 11 / 14 -> 10 * 11 / 14 = 7.857... -> round 7.9
        var result2 = AttendanceCalculator.Calculate(present: 11, late: 0, excused: 0, absent: 3, tongBuoi: 14, _defaultOptions);
        Assert.Equal(7.9, result2.DiemGoiY);
    }
}
