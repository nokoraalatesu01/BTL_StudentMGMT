using System;
using System.Collections.Generic;
using System.Linq;

namespace BTL_StudentMGMT.Models;

public class CaHocInfo
{
    public int Ca { get; set; } // 1, 2, 3, 4
    public string MaCa { get; set; } = string.Empty; // "CA12", "CA34", "CA56", "CA78"
    public string TenCa { get; set; } = string.Empty; // "Ca 12", "Ca 34", "Ca 56", "Ca 78"
    public int TietBatDau { get; set; }
    public int TietKetThuc { get; set; }
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public string KhungGio => $"{GioBatDau} – {GioKetThuc}";
    public string MoTaTiet => $"Tiết {TietBatDau}–{TietKetThuc}";
    public string? GiaiLaoSauCa { get; set; }
}

public static class CaHocHelper
{
    // 4 ca học cố định chuẩn theo quy ước đào tạo UTC
    public static readonly List<CaHocInfo> DanhSachCa = new()
    {
        new CaHocInfo
        {
            Ca = 1,
            MaCa = "CA12",
            TenCa = "Ca 12",
            TietBatDau = 1,
            TietKetThuc = 2,
            GioBatDau = "07:00",
            GioKetThuc = "09:30",
            GiaiLaoSauCa = "Nghỉ giải lao: 09:30 – 09:35 (5 phút)"
        },
        new CaHocInfo
        {
            Ca = 2,
            MaCa = "CA34",
            TenCa = "Ca 34",
            TietBatDau = 3,
            TietKetThuc = 4,
            GioBatDau = "09:35",
            GioKetThuc = "12:00",
            GiaiLaoSauCa = "Nghỉ trưa: 12:00 – 13:00 (60 phút)"
        },
        new CaHocInfo
        {
            Ca = 3,
            MaCa = "CA56",
            TenCa = "Ca 56",
            TietBatDau = 5,
            TietKetThuc = 6,
            GioBatDau = "13:00",
            GioKetThuc = "15:10",
            GiaiLaoSauCa = "Nghỉ giải lao: 15:10 – 15:35 (25 phút)"
        },
        new CaHocInfo
        {
            Ca = 4,
            MaCa = "CA78",
            TenCa = "Ca 78",
            TietBatDau = 7,
            TietKetThuc = 8,
            GioBatDau = "15:35",
            GioKetThuc = "18:00",
            GiaiLaoSauCa = null
        }
    };

    // Bảng thời gian chi tiết từng tiết học chuẩn UTC
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

    // Bảng màu CSS Tint
    public static readonly string[] CourseColorClasses = new[]
    {
        "tint-blue",
        "tint-green",
        "tint-purple",
        "tint-amber",
        "tint-rose",
        "tint-teal",
        "tint-indigo",
        "tint-slate"
    };

    // Bảng màu sắc nhận diện UTC cho thẻ sự kiện môn học (Solid Header & Border)
    public static readonly (string HeaderBg, string BorderColor, string TagBg)[] UtcThemeColors = new[]
    {
        ("#ea580c", "#c2410c", "#ffedd5"), // Orange (như ảnh UTC: Thuật toán & ứng dụng)
        ("#d97706", "#b45309", "#fef3c7"), // Amber/Bronze (như ảnh UTC: Mạng máy tính)
        ("#1e40af", "#1d4ed8", "#dbeafe"), // Navy Blue (như ảnh UTC: Phân tích thiết kế)
        ("#15803d", "#166534", "#dcfce7"), // Emerald Green
        ("#7c3aed", "#6d28d9", "#ede9fe"), // Violet/Purple
        ("#0891b2", "#0e7490", "#cffafe"), // Cyan
        ("#be185d", "#9d174d", "#fce7f3"), // Rose/Pink
        ("#4f46e5", "#4338ca", "#e0e7ff")  // Indigo
    };

    public static (string HeaderBg, string BorderColor, string TagBg) GetUtcThemeColor(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return UtcThemeColors[0];
        uint hash = 0;
        foreach (char c in code)
        {
            hash = (hash * 31) + c;
        }
        var index = (int)(hash % (uint)UtcThemeColors.Length);
        return UtcThemeColors[index];
    }

    public static (int TopPx, int HeightPx, string StartStr, string EndStr) CalculateTimelinePosition(int tietBatDau, int soTiet)
    {
        int tietKetThuc = tietBatDau + soTiet - 1;
        string startStr;
        string endStr;
        int top;
        int height;

        // Bảng quy chuẩn giờ học UTC:
        // Ca 12: 07:00 - 09:30; Ca 34: 09:35 - 12:00; Ca 56: 13:00 - 15:10; Ca 78: 15:35 - 18:00
        // Timeline lưới giờ: Mỗi giờ = 50px (từ 7:00 đến 18:00 = 11 block x 50px = 550px)
        if (tietBatDau <= 2)
        {
            top = 0; // 07:00
            startStr = "07:00";
            if (tietKetThuc >= 4)
            {
                // Kéo dài cả Ca 12 và Ca 34 (Tiết 1-4: 07:00 - 12:00 = 5 giờ x 50px = 250px)
                height = 250;
                endStr = "12:00";
            }
            else
            {
                // Ca 12 (07:00 - 09:30 = 2.5 giờ x 50px = 125px)
                height = 125;
                endStr = "09:30";
            }
        }
        else if (tietBatDau <= 4)
        {
            // Ca 34 (09:35 - 12:00 = top 130px, height 120px)
            top = 130;
            height = 120;
            startStr = "09:35";
            endStr = "12:00";
        }
        else if (tietBatDau <= 6)
        {
            // Ca 56 (13:00 - 15:10)
            top = 300; // 13:00 (cách 7:00 đúng 6 giờ = 300px)
            startStr = "13:00";
            if (tietKetThuc >= 8)
            {
                // Kéo dài cả Ca 56 và Ca 78 (Tiết 5-8: 13:00 - 18:00 = 5 giờ x 50px = 250px)
                height = 250;
                endStr = "18:00";
            }
            else
            {
                // Ca 56 (13:00 - 15:10)
                height = 115;
                endStr = "15:10";
            }
        }
        else
        {
            // Ca 78 (15:35 - 18:00)
            top = 430; // 15:35
            height = 120;
            startStr = "15:35";
            endStr = "18:00";
        }

        return (top, height, startStr, endStr);
    }

    public static string GetKhungGio(int tietBatDau, int soTiet)
    {
        var tietKetThuc = Math.Max(tietBatDau, tietBatDau + soTiet - 1);
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
        if (result.Count == 0)
        {
            if (tietBatDau <= 2) result.Add(1);
            else if (tietBatDau <= 4) result.Add(2);
            else if (tietBatDau <= 6) result.Add(3);
            else result.Add(4);
        }
        return result;
    }

    public static string GetTenCa(int ca) => ca switch
    {
        1 => "Ca 12",
        2 => "Ca 34",
        3 => "Ca 56",
        4 => "Ca 78",
        _ => $"Ca {ca}"
    };

    public static string GetCourseColorClass(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return CourseColorClasses[0];
        uint hash = 0;
        foreach (char c in code)
        {
            hash = (hash * 31) + c;
        }
        var index = (int)(hash % (uint)CourseColorClasses.Length);
        return CourseColorClasses[index];
    }

    public static (string StatusCode, string StatusText, string BadgeClass) GetSessionStatus(DateTime ngayHoc, int tietBatDau, int soTiet, string? lhpTrangThai)
    {
        if (string.Equals(lhpTrangThai, "CANCELLED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lhpTrangThai, "DA_HUY", StringComparison.OrdinalIgnoreCase))
        {
            return ("DA_HUY", "Đã hủy", "badge-status-cancelled");
        }
        if (string.Equals(lhpTrangThai, "RESCHEDULED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lhpTrangThai, "DOI_LICH", StringComparison.OrdinalIgnoreCase))
        {
            return ("DOI_LICH", "Đã đổi lịch", "badge-status-rescheduled");
        }

        var today = DateTime.Today;
        var dateOnly = ngayHoc.Date;

        if (dateOnly < today)
        {
            return ("DA_HOAN_THANH", "Đã kết thúc", "badge-status-completed");
        }
        if (dateOnly > today)
        {
            return ("SAP_DIEN_RA", "Sắp diễn ra", "badge-status-upcoming");
        }

        // Cùng ngày hôm nay -> kiểm tra giờ thực tế
        var tietKetThuc = tietBatDau + soTiet - 1;
        var startStr = GioCacTiet.ContainsKey(tietBatDau) ? GioCacTiet[tietBatDau].Start : "07:00";
        var endStr = GioCacTiet.ContainsKey(tietKetThuc) ? GioCacTiet[tietKetThuc].End : "18:00";

        if (TimeSpan.TryParse(startStr, out var startTime) && TimeSpan.TryParse(endStr, out var endTime))
        {
            var nowTime = DateTime.Now.TimeOfDay;
            if (nowTime >= startTime && nowTime <= endTime)
            {
                return ("DANG_DIEN_RA", "Đang diễn ra", "badge-status-ongoing");
            }
            if (nowTime > endTime)
            {
                return ("DA_HOAN_THANH", "Đã kết thúc", "badge-status-completed");
            }
        }

        return ("SAP_DIEN_RA", "Hôm nay", "badge-status-upcoming");
    }

    public static DayOfWeek? ParseThu(string? thuStr)
    {
        if (string.IsNullOrWhiteSpace(thuStr)) return null;
        var s = thuStr.Trim().ToLowerInvariant();
        if (s.Contains("2") || s.Contains("hai")) return DayOfWeek.Monday;
        if (s.Contains("3") || s.Contains("ba")) return DayOfWeek.Tuesday;
        if (s.Contains("4") || s.Contains("tư")) return DayOfWeek.Wednesday;
        if (s.Contains("5") || s.Contains("năm")) return DayOfWeek.Thursday;
        if (s.Contains("6") || s.Contains("sáu")) return DayOfWeek.Friday;
        if (s.Contains("7") || s.Contains("bảy")) return DayOfWeek.Saturday;
        if (s.Contains("chủ nhật") || s.Contains("cn") || s.Contains("nhật")) return DayOfWeek.Sunday;
        return null;
    }
}

public class ThoiKhoaBieuCellViewModel
{
    public string Thu { get; set; } = string.Empty;
    public int Ca { get; set; } // 1: Ca 12, 2: Ca 34, 3: Ca 56, 4: Ca 78
    public int RowSpan { get; set; } = 1;
    public bool IsCoveredByRowSpan { get; set; } = false; // Nếu true: ô này đã bị ô ở Ca trước chiếm chỗ bởi rowspan, không render <td>
    public List<ThoiKhoaBieuCardViewModel> DanhSachLop { get; set; } = new();
}

public class SinhVienSimpleItemDto
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? TenLopHanhChinh { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string TrangThaiDiemDanh { get; set; } = "Chưa điểm danh";
}

public class LopHocPhanItemDto
{
    public int Id { get; set; }
    public string MaLopHocPhan { get; set; } = string.Empty;
    public string TenLopHocPhan { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;
    public string MaMon { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public string HinhThucHoc { get; set; } = "Thực hành";
    public string NgayBatDau { get; set; } = "10/08/2026";
    public string NgayKetThuc { get; set; } = "22/11/2026";
    public string GhiChu { get; set; } = "Chính khóa";
    public string? TenGiangVien { get; set; }
    public string? PhongHoc { get; set; }
    public string? ThoiGianHoc { get; set; }
    public string? LopPhuTrach { get; set; }
    public int SiSo { get; set; }
    public bool DangChon { get; set; }
}

public class ThoiKhoaBieuCardViewModel
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = string.Empty;
    public string TenLopHocPhan { get; set; } = string.Empty;
    public string NhomLop { get; set; } = "01";
    public int MonHocId { get; set; }
    public string MaMon { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public int? GiangVienId { get; set; }
    public string? GiangVien { get; set; }
    public string? EmailGiangVien { get; set; }
    public string? SoDienThoaiGiangVien { get; set; }
    public string Thu { get; set; } = string.Empty; // "Thứ 2" -> "Chủ nhật"
    public DateTime? NgayHoc { get; set; }
    public int TietBatDau { get; set; }
    public int SoTiet { get; set; }
    public int TietKetThuc => Math.Max(TietBatDau, TietBatDau + SoTiet - 1);
    public string KhungGio { get; set; } = string.Empty;
    public string PhongHoc { get; set; } = string.Empty;
    public int CaHoc { get; set; } // 1, 2, 3, 4
    public string TenCaHoc { get; set; } = string.Empty; // "Ca 12", "Ca 34",...
    public int CaHocGoc { get; set; }
    public bool LaCaKeoDai { get; set; }
    public int RowSpan { get; set; } = 1;
    public string? GhiChuGiaiLao { get; set; }
    public string StatusCode { get; set; } = "SAP_DIEN_RA";
    public string StatusText { get; set; } = "Sắp diễn ra";
    public string BadgeClass { get; set; } = "badge-status-upcoming";
    public string ColorClass { get; set; } = "tint-blue";

    // Thuộc tính hiển thị timeline chuẩn nhận diện UTC (Solid color header & border)
    public int TimelineTop { get; set; } = 0;
    public int TimelineHeight { get; set; } = 125;
    public string ThemeHeaderColor { get; set; } = "#ea580c";
    public string ThemeBorderColor { get; set; } = "#c2410c";
    public string ThemeTagBg { get; set; } = "#ffedd5";
    public string TimelineColStyle { get; set; } = "left: 3px; right: 3px;";

    public bool BiTrungLich { get; set; }
    public string? LyDoTrungLich { get; set; }
    public string? GhiChu { get; set; }
    public int SiSo { get; set; }
    public int SiSoToiDa { get; set; } = 60;
    public bool DaDiemDanh { get; set; }
    public string TrangThaiDiemDanhCuaSinhVien { get; set; } = "Chưa điểm danh"; // Dành riêng cho sinh viên
    public List<SinhVienSimpleItemDto> DanhSachSinhVien { get; set; } = new();
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
    public string? PhongHocTrung { get; set; }
    public string LoaiTrung { get; set; } = "TRUNG_TIET"; // TRUNG_TIET, TRUNG_PHONG
}

public class NgayTrongTuanViewModel
{
    public string Thu { get; set; } = string.Empty; // "Thứ 2" -> "Chủ nhật"
    public string TenRutGon { get; set; } = string.Empty; // "T2", "T3",... "CN"
    public DateTime Ngay { get; set; }
    public bool LaHomNay { get; set; }
    public int TongSoLop { get; set; }
}

public class NgayTrongThangViewModel
{
    public DateTime Ngay { get; set; }
    public bool LaThangHienTai { get; set; }
    public bool LaHomNay { get; set; }
    public bool LaTuanHienTai { get; set; }
    public string NgayUrlParam => Ngay.ToString("yyyy-MM-dd");
    public List<ThoiKhoaBieuCardViewModel> DanhSachBuoiHoc { get; set; } = new();
    public int TongBuoiHoc => DanhSachBuoiHoc.Count;
}

public class TuanTrongThangViewModel
{
    public int TuanSo { get; set; }
    public List<NgayTrongThangViewModel> CacNgay { get; set; } = new(); // 7 ngày: T2 đến CN
}

public class ThangCalendarViewModel
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public string TieuDeThang => $"Tháng {Thang:D2}/{Nam}";
    public DateTime NgayDauThang { get; set; }
    public DateTime NgayCuoiThang { get; set; }
    public List<TuanTrongThangViewModel> CacTuan { get; set; } = new();
    public string ThangTruocParam => NgayDauThang.AddMonths(-1).ToString("yyyy-MM-dd");
    public string ThangSauParam => NgayDauThang.AddMonths(1).ToString("yyyy-MM-dd");
    public string ThangHienTaiParam => DateTime.Today.ToString("yyyy-MM-dd");
}

public class SinhVienThoiKhoaBieuViewModel
{
    public SinhVien SinhVien { get; set; } = null!;
    public HocKy? HocKyHienTai { get; set; }
    public List<HocKy> DanhSachHocKy { get; set; } = new();
    public NamHoc? NamHocHienTai { get; set; }
    public List<NamHoc> DanhSachNamHoc { get; set; } = new();
    public List<MonHoc> DanhSachMonHoc { get; set; } = new();
    public int? SelectedNamHocId { get; set; }
    public int? SelectedHocKyId { get; set; }
    public int? SelectedMonHocId { get; set; }

    public string ViewMode { get; set; } = "week"; // "week" hoặc "month"

    // Điều hướng tuần
    public DateTime NgayDauTuan { get; set; } // Thứ 2
    public DateTime NgayCuoiTuan { get; set; } // Chủ Nhật
    public int TuanHienTai { get; set; }
    public string ChuoiTuanHienThi { get; set; } = string.Empty;
    public string NgayTruocUrlParam => NgayDauTuan.AddDays(-7).ToString("yyyy-MM-dd");
    public string NgaySauUrlParam => NgayDauTuan.AddDays(7).ToString("yyyy-MM-dd");
    public string NgayHienTaiUrlParam => DateTime.Today.ToString("yyyy-MM-dd");
    public string NgayDauTuanParam => NgayDauTuan.ToString("yyyy-MM-dd");

    // Bảng danh sách lớp học phần (hiển thị phía trên lịch theo phong cách UTC)
    public List<LopHocPhanItemDto> DanhSachLopHocPhan { get; set; } = new();

    // Lịch tháng mini widget (phía trên bên phải theo phong cách UTC)
    public ThangCalendarViewModel? MiniCalendar { get; set; }

    // Danh sách thẻ sự kiện theo từng thứ trong tuần (định vị theo timeline giờ học)
    public Dictionary<string, List<ThoiKhoaBieuCardViewModel>> TimelineCardsByDay { get; set; } = new();

    // Lịch tháng chế độ xem đầy đủ
    public ThangCalendarViewModel? ThangView { get; set; }

    // Thống kê
    public int TongLopHocPhan { get; set; }
    public int TongBuoiHocTrongTuan { get; set; }
    public int TongTinChiDangKy { get; set; }

    // 7 ngày trong tuần: Thứ 2 đến Chủ nhật
    public List<NgayTrongTuanViewModel> CacNgayTrongTuan { get; set; } = new();

    // Cảnh báo trùng lịch
    public List<TrungLichItemViewModel> DanhSachTrungLich { get; set; } = new();

    // Ma trận các Cell chuẩn (hỗ trợ liên tục qua nhiều ca với RowSpan, không lặp thẻ)
    public Dictionary<string, ThoiKhoaBieuCellViewModel> MaTranTuan { get; set; } = new();

    // Ma trận thời khóa biểu tuần: Key = "{Thu}_Ca{ca}"
    public Dictionary<string, List<ThoiKhoaBieuCardViewModel>> MaTranThoiKhoaBieu { get; set; } = new();

    // Danh sách phẳng
    public List<ThoiKhoaBieuCardViewModel> TatCaLopHoc { get; set; } = new();

    public bool ChuaDangKyMonNao => TongLopHocPhan == 0;
    public bool ChuaCoLichHoc => TongLopHocPhan > 0 && TatCaLopHoc.Count == 0;
}

public class GiangVienLichGiangDayViewModel
{
    public GiangVien GiangVien { get; set; } = null!;
    public HocKy? HocKyHienTai { get; set; }
    public List<HocKy> DanhSachHocKy { get; set; } = new();
    public NamHoc? NamHocHienTai { get; set; }
    public List<NamHoc> DanhSachNamHoc { get; set; } = new();
    public List<MonHoc> DanhSachMonHoc { get; set; } = new();
    public int? SelectedNamHocId { get; set; }
    public int? SelectedHocKyId { get; set; }
    public int? SelectedMonHocId { get; set; }

    public string ViewMode { get; set; } = "week"; // "week" hoặc "month"

    // Điều hướng tuần
    public DateTime NgayDauTuan { get; set; } // Thứ 2
    public DateTime NgayCuoiTuan { get; set; } // Chủ Nhật
    public string ChuoiTuanHienThi { get; set; } = string.Empty;
    public string NgayTruocUrlParam => NgayDauTuan.AddDays(-7).ToString("yyyy-MM-dd");
    public string NgaySauUrlParam => NgayDauTuan.AddDays(7).ToString("yyyy-MM-dd");
    public string NgayHienTaiUrlParam => DateTime.Today.ToString("yyyy-MM-dd");
    public string NgayDauTuanParam => NgayDauTuan.ToString("yyyy-MM-dd");

    // Bảng danh sách lớp học phần (hiển thị phía trên lịch theo phong cách UTC)
    public List<LopHocPhanItemDto> DanhSachLopHocPhan { get; set; } = new();

    // Lịch tháng mini widget (phía trên bên phải theo phong cách UTC)
    public ThangCalendarViewModel? MiniCalendar { get; set; }

    // Danh sách thẻ sự kiện theo từng thứ trong tuần (định vị theo timeline giờ học)
    public Dictionary<string, List<ThoiKhoaBieuCardViewModel>> TimelineCardsByDay { get; set; } = new();

    // Lịch tháng chế độ xem đầy đủ
    public ThangCalendarViewModel? ThangView { get; set; }

    // Thống kê
    public int TongMonPhuTrach { get; set; }
    public int TongLopPhuTrach { get; set; }
    public int TongBuoiDayTrongTuan { get; set; }
    public int TongSinhVienPhuTrach { get; set; }

    // 7 ngày trong tuần: Thứ 2 đến Chủ nhật
    public List<NgayTrongTuanViewModel> CacNgayTrongTuan { get; set; } = new();

    // Cảnh báo trùng lịch
    public List<TrungLichItemViewModel> DanhSachTrungLich { get; set; } = new();

    // Ma trận các Cell chuẩn (hỗ trợ liên tục qua nhiều ca với RowSpan, không lặp thẻ)
    public Dictionary<string, ThoiKhoaBieuCellViewModel> MaTranTuan { get; set; } = new();

    // Ma trận thời khóa biểu tuần: Key = "{Thu}_Ca{ca}"
    public Dictionary<string, List<ThoiKhoaBieuCardViewModel>> MaTranThoiKhoaBieu { get; set; } = new();

    // Danh sách phẳng tất cả buổi dạy
    public List<ThoiKhoaBieuCardViewModel> TatCaBuoiDay { get; set; } = new();

    public bool ChuaPhuTrachLopNao => TongLopPhuTrach == 0;
}
