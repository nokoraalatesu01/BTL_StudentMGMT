using System;
using System.Collections.Generic;
using System.Linq;

namespace BTL_StudentMGMT.Models;

public enum AttendanceWarningLevel
{
    BinhThuong = 0,
    CanhBao = 1,
    NguyCo = 2
}

public class AttendanceCalculationResult
{
    public int TongBuoi { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public int Absent { get; set; }
    public double PhanTramVang { get; set; }
    public AttendanceWarningLevel MucCanhBao { get; set; }
    public string TenMucCanhBao { get; set; } = "Bình thường";
    public string BadgeClass { get; set; } = "bg-success text-white";

    // Gợi ý điểm chuyên cần (G12)
    public double? DiemGoiY { get; set; }
    public string? DienGiaiGoiY { get; set; }
    public int SoBuoiCoMat { get; set; }
}

public static class AttendanceCalculator
{
    public static AttendanceCalculationResult Calculate(
        int present,
        int late,
        int excused,
        int absent,
        int tongBuoi,
        AttendanceOptions? options = null)
    {
        options ??= new AttendanceOptions();

        var result = new AttendanceCalculationResult
        {
            TongBuoi = Math.Max(0, tongBuoi),
            Present = Math.Max(0, present),
            Late = Math.Max(0, late),
            Excused = Math.Max(0, excused),
            Absent = Math.Max(0, absent)
        };

        if (result.TongBuoi == 0)
        {
            result.PhanTramVang = 0.0;
            result.MucCanhBao = AttendanceWarningLevel.BinhThuong;
            result.TenMucCanhBao = GetTenMucCanhBao(AttendanceWarningLevel.BinhThuong);
            result.BadgeClass = GetBadgeClass(AttendanceWarningLevel.BinhThuong);
            result.DiemGoiY = null;
            result.DienGiaiGoiY = null;
            result.SoBuoiCoMat = 0;
            return result;
        }

        // Tính % vắng = (Absent + (CountExcusedAsAbsent ? Excused : 0)) / TongBuoi * 100
        double soBuoiVangTinh = result.Absent + (options.CountExcusedAsAbsent ? result.Excused : 0);
        result.PhanTramVang = Math.Round(soBuoiVangTinh * 100.0 / result.TongBuoi, 1);

        // Mức cảnh báo:
        // Không cảnh báo khi TongBuoi < MinSessionsForWarning
        if (result.TongBuoi < options.MinSessionsForWarning)
        {
            result.MucCanhBao = AttendanceWarningLevel.BinhThuong;
        }
        else
        {
            double warningThreshold = 0.7 * options.AbsenceThresholdPercent;
            // Dùng so sánh trực tiếp theo tỷ lệ không làm tròn trước để đảm bảo tính chuẩn xác
            double phanTramExact = soBuoiVangTinh * 100.0 / result.TongBuoi;

            if (phanTramExact >= (double)options.AbsenceThresholdPercent - 1e-9)
            {
                result.MucCanhBao = AttendanceWarningLevel.NguyCo;
            }
            else if (phanTramExact >= warningThreshold - 1e-9)
            {
                result.MucCanhBao = AttendanceWarningLevel.CanhBao;
            }
            else
            {
                result.MucCanhBao = AttendanceWarningLevel.BinhThuong;
            }
        }

        result.TenMucCanhBao = GetTenMucCanhBao(result.MucCanhBao);
        result.BadgeClass = GetBadgeClass(result.MucCanhBao);

        // Gợi ý điểm chuyên cần (G12):
        // round(10 * (Present + Late + (CountExcusedAsAbsent ? 0 : Excused)) / TongBuoi, 1)
        int soBuoiCoMat = result.Present + result.Late + (options.CountExcusedAsAbsent ? 0 : result.Excused);
        result.SoBuoiCoMat = soBuoiCoMat;
        result.DiemGoiY = Math.Round(10.0 * soBuoiCoMat / result.TongBuoi, 1);
        result.DienGiaiGoiY = $"dựa trên {soBuoiCoMat}/{result.TongBuoi} buổi";

        return result;
    }

    public static AttendanceCalculationResult Calculate(
        IEnumerable<DiemDanh> records,
        int tongBuoi,
        AttendanceOptions? options = null)
    {
        int present = 0, late = 0, excused = 0, absent = 0;
        foreach (var r in records)
        {
            var st = AttendanceStatus.Normalize(r.TrangThai);
            if (st == AttendanceStatus.Present) present++;
            else if (st == AttendanceStatus.Late) late++;
            else if (st == AttendanceStatus.Excused) excused++;
            else if (st == AttendanceStatus.Absent) absent++;
        }
        return Calculate(present, late, excused, absent, tongBuoi, options);
    }

    public static AttendanceCalculationResult Calculate(
        IEnumerable<string> statuses,
        int tongBuoi,
        AttendanceOptions? options = null)
    {
        int present = 0, late = 0, excused = 0, absent = 0;
        foreach (var s in statuses)
        {
            var st = AttendanceStatus.Normalize(s);
            if (st == AttendanceStatus.Present) present++;
            else if (st == AttendanceStatus.Late) late++;
            else if (st == AttendanceStatus.Excused) excused++;
            else if (st == AttendanceStatus.Absent) absent++;
        }
        return Calculate(present, late, excused, absent, tongBuoi, options);
    }

    public static string GetTenMucCanhBao(AttendanceWarningLevel level) => level switch
    {
        AttendanceWarningLevel.NguyCo => "Nguy cơ cấm thi",
        AttendanceWarningLevel.CanhBao => "Cảnh báo vắng",
        _ => "Bình thường"
    };

    public static string GetBadgeClass(AttendanceWarningLevel level) => level switch
    {
        AttendanceWarningLevel.NguyCo => "badge-canhbao-nguyco bg-danger text-white",
        AttendanceWarningLevel.CanhBao => "badge-canhbao-canhbao bg-warning text-dark",
        _ => "badge-canhbao-binhthuong bg-success text-white"
    };
}
