using System;
using System.Collections.Generic;
using System.Linq;

namespace BTL_StudentMGMT.Models;

public class CaHocInfo
{
    public int Ca { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public int TietBatDau { get; set; }
    public int TietKetThuc { get; set; }
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public string KhungGio => $"{GioBatDau} – {GioKetThuc}";
    public string MoTaTiet => $"Tiết {TietBatDau}–{TietKetThuc}";
}

public static class CaHocHelper
{
    // 4 ca học cố định theo quy định đào tạo
    public static readonly List<CaHocInfo> DanhSachCa = new()
    {
        new CaHocInfo { Ca = 1, TenCa = "Ca 1", TietBatDau = 1, TietKetThuc = 2, GioBatDau = "07:00", GioKetThuc = "09:30" },
        new CaHocInfo { Ca = 2, TenCa = "Ca 2", TietBatDau = 3, TietKetThuc = 4, GioBatDau = "09:35", GioKetThuc = "12:00" },
        new CaHocInfo { Ca = 3, TenCa = "Ca 3", TietBatDau = 5, TietKetThuc = 6, GioBatDau = "13:00", GioKetThuc = "15:10" },
        new CaHocInfo { Ca = 4, TenCa = "Ca 4", TietBatDau = 7, TietKetThuc = 8, GioBatDau = "15:35", GioKetThuc = "18:00" }
    };

    // Bảng giờ chi tiết từng tiết học phục vụ tính toán khung giờ chính xác
    private static readonly Dictionary<int, (string Start, string End)> GioCacTiet = new()
    {
        { 1, ("07:00", "08:10") },
        { 2, ("08:20", "09:30") },
        { 3, ("09:35", "10:45") },
        { 4, ("10:50", "12:00") },
        { 5, ("13:00", "14:00") },
        { 6, ("14:10", "15:10") },
        { 7, ("15:35", "16:45") },
        { 8, ("16:50", "18:00") },
        { 9, ("18:15", "19:15") },
        { 10, ("19:20", "20:20") },
        { 11, ("20:25", "21:15") },
        { 12, ("21:20", "22:10") }
    };

    public static string GetKhungGio(int tietBatDau, int soTiet)
    {
        var tietKetThuc = tietBatDau + soTiet - 1;
        var start = GioCacTiet.ContainsKey(tietBatDau) ? GioCacTiet[tietBatDau].Start : "07:00";
        var end = GioCacTiet.ContainsKey(tietKetThuc) ? GioCacTiet[tietKetThuc].End : "18:00";
        return $"{start} – {end}";
    }

    public static List<int> GetCacCaGiaoNhau(int tietBatDau, int tietKetThuc)
    {
        var result = new List<int>();
        foreach (var ca in DanhSachCa)
        {
            if (Math.Max(tietBatDau, ca.TietBatDau) <= Math.Min(tietKetThuc, ca.TietKetThuc))
            {
                result.Add(ca.Ca);
            }
        }
        // Trường hợp học ngoài 4 ca chính (ví dụ tiết 9-10) thì map vào Ca 4
        if (result.Count == 0 && tietBatDau >= 7)
        {
            result.Add(4);
        }
        return result;
    }
}

public class ThoiKhoaBieuCardViewModel
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = string.Empty;
    public string TenLopHocPhan { get; set; } = string.Empty;
    public string MaMon { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public string? GiangVien { get; set; }
    public string? EmailGiangVien { get; set; }
    public string? SoDienThoaiGiangVien { get; set; }
    public string Thu { get; set; } = string.Empty; // "Thứ 2", "Thứ 3",...
    public DateTime? NgayHoc { get; set; } // Ngày cụ thể trong tuần
    public int TietBatDau { get; set; }
    public int SoTiet { get; set; }
    public int TietKetThuc => TietBatDau + SoTiet - 1;
    public string KhungGio { get; set; } = string.Empty;
    public string PhongHoc { get; set; } = string.Empty;
    public int CaHoc { get; set; } // 1, 2, 3, 4 (Ca đang hiển thị)
    public int CaHocGoc { get; set; } // Ca bắt đầu buổi học
    public bool LaCaKeoDai { get; set; } // Buổi học bắt đầu từ ca trước kéo dài sang ca này
    public bool BiTrungLich { get; set; } // Có bị trùng với lớp khác không
    public string? LyDoTrungLich { get; set; }
    public string? GhiChu { get; set; }
}

public class TrungLichItemViewModel
{
    public string Thu { get; set; } = string.Empty;
    public string LopA { get; set; } = string.Empty;
    public string MonA { get; set; } = string.Empty;
    public string TietA { get; set; } = string.Empty;
    public string LopB { get; set; } = string.Empty;
    public string MonB { get; set; } = string.Empty;
    public string TietB { get; set; } = string.Empty;
    public string TietTrung { get; set; } = string.Empty;
}

public class NgayTrongTuanViewModel
{
    public string Thu { get; set; } = string.Empty; // "Thứ 2", "Thứ 3",...
    public string TenRutGon { get; set; } = string.Empty; // "T2", "T3",...
    public DateTime Ngay { get; set; }
    public bool LaHomNay { get; set; }
    public int TongSoLop { get; set; }
}

public class SinhVienThoiKhoaBieuViewModel
{
    public SinhVien SinhVien { get; set; } = null!;
    public HocKy? HocKyHienTai { get; set; }
    public List<HocKy> DanhSachHocKy { get; set; } = new();

    // Điều hướng tuần
    public DateTime NgayDauTuan { get; set; } // Thứ 2
    public DateTime NgayCuoiTuan { get; set; } // Chủ Nhật
    public int TuanHienTai { get; set; }
    public string ChuoiTuanHienThi { get; set; } = string.Empty; // "Tuần 02/11/2026 – 08/11/2026"
    public string NgayTruocUrlParam => NgayDauTuan.AddDays(-7).ToString("yyyy-MM-dd");
    public string NgaySauUrlParam => NgayDauTuan.AddDays(7).ToString("yyyy-MM-dd");
    public string NgayHienTaiUrlParam => DateTime.Today.ToString("yyyy-MM-dd");

    // 3 Thẻ thống kê
    public int TongLopHocPhan { get; set; }
    public int TongBuoiHocTrongTuan { get; set; }
    public int TongTinChiDangKy { get; set; }

    // Danh sách ngày trong tuần (Thứ 2 đến Thứ 7)
    public List<NgayTrongTuanViewModel> CacNgayTrongTuan { get; set; } = new();

    // Cảnh báo trùng lịch
    public List<TrungLichItemViewModel> DanhSachTrungLich { get; set; } = new();

    // Dữ liệu ma trận: Key = (Thu, Ca) -> Danh sách Card
    // Dictionary<(Thu, Ca), List<ThoiKhoaBieuCardViewModel>>
    public Dictionary<string, List<ThoiKhoaBieuCardViewModel>> MaTranThoiKhoaBieu { get; set; } = new();

    // Danh sách phẳng tất cả các lớp học phần sinh viên đăng ký để hiển thị modal / danh sách mobile
    public List<ThoiKhoaBieuCardViewModel> TatCaLopHoc { get; set; } = new();

    // Trạng thái dữ liệu
    public bool ChuaDangKyMonNao => TongLopHocPhan == 0;
    public bool ChuaCoLichHoc => TongLopHocPhan > 0 && TatCaLopHoc.Count == 0;
}
