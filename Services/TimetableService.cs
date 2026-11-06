using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Services;

public class TimetableService : ITimetableService
{
    private readonly ApplicationDbContext _db;

    public TimetableService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<GiangVienLichGiangDayViewModel> GetGiangVienLichGiangDayAsync(
        int giangVienId,
        int? hocKyId = null,
        int? namHocId = null,
        int? monHocId = null,
        string? ngay = null,
        string? viewMode = "week")
    {
        var giangVien = await _db.GiangViens
            .AsNoTracking()
            .Include(g => g.DonVi)
            .FirstOrDefaultAsync(g => g.Id == giangVienId)
            ?? throw new ArgumentException("Không tìm thấy thông tin giảng viên");

        // 1. Danh sách Học kỳ & Năm học để lọc
        var allNamHocs = await _db.NamHocs.AsNoTracking().OrderByDescending(n => n.NgayBatDau).ToListAsync();
        var allHocKies = await _db.HocKies.AsNoTracking().Include(h => h.NamHoc).OrderByDescending(h => h.NgayBatDau).ToListAsync();

        // Xác định học kỳ mặc định
        HocKy? selectedHocKy = null;
        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            selectedHocKy = allHocKies.FirstOrDefault(h => h.HocKyId == hocKyId.Value);
        }
        else if (namHocId.HasValue && namHocId.Value > 0)
        {
            selectedHocKy = allHocKies.FirstOrDefault(h => h.NamHocId == namHocId.Value);
        }

        if (selectedHocKy == null)
        {
            var todayOnly = DateOnly.FromDateTime(DateTime.Today);
            selectedHocKy = allHocKies.FirstOrDefault(h => h.TrangThai == "DANG_MO" || (h.NgayBatDau <= todayOnly && h.NgayKetThuc >= todayOnly))
                            ?? allHocKies.FirstOrDefault();
        }

        var selectedNamHoc = selectedHocKy != null ? allNamHocs.FirstOrDefault(n => n.NamHocId == selectedHocKy.NamHocId) : allNamHocs.FirstOrDefault();

        // 2. Query danh sách lớp học phần của giảng viên
        var lhpQuery = _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy).ThenInclude(h => h!.NamHoc)
            .Include(l => l.DangKyHocPhans).ThenInclude(dk => dk.SinhVien).ThenInclude(sv => sv.Lop)
            .Where(l => l.GiangVienId == giangVienId)
            .AsQueryable();

        if (selectedHocKy != null)
        {
            lhpQuery = lhpQuery.Where(l => l.HocKyId == selectedHocKy.HocKyId);
        }

        if (monHocId.HasValue && monHocId.Value > 0)
        {
            lhpQuery = lhpQuery.Where(l => l.MonHocId == monHocId.Value);
        }

        var rawLhpList = await lhpQuery.ToListAsync();

        // Lấy danh sách tất cả các môn học mà giảng viên phụ trách (để hiển thị filter)
        var dsMonHocCuaGv = await _db.LopHocPhans
            .AsNoTracking()
            .Where(l => l.GiangVienId == giangVienId && l.MonHoc != null)
            .Select(l => l.MonHoc!)
            .Distinct()
            .OrderBy(m => m.TenMon)
            .ToListAsync();

        // 3. Tính toán thời gian tuần & tháng
        var (monday, sunday, targetDate) = CalculateWeekBounds(ngay);

        var dateMap = BuildDateMap(monday);
        var cacThuConfig = BuildWeekDays(monday);

        // 4. Kiểm tra trùng lịch giảng dạy
        var (trungLichIds, danhSachTrungLich) = CheckLecturerConflicts(rawLhpList);

        // 5. Build Cards & Ma trận tuần
        var (maTranTuan, maTranCu, tatCaCards) = await BuildTimetableCardsAsync(
            rawLhpList,
            cacThuConfig,
            dateMap,
            trungLichIds,
            danhSachTrungLich,
            isLecturer: true,
            currentStudentId: null);

        // 6. Xây dựng Lịch tháng nếu viewMode == "month"
        ThangCalendarViewModel? thangView = null;
        if (string.Equals(viewMode, "month", StringComparison.OrdinalIgnoreCase))
        {
            thangView = await BuildMonthCalendarAsync(
                targetDate,
                rawLhpList,
                trungLichIds,
                danhSachTrungLich,
                isLecturer: true,
                currentStudentId: null,
                currentMonday: monday,
                currentSunday: sunday);
        }

        // Mini Calendar Widget chuẩn UTC
        var miniCalendar = await BuildMonthCalendarAsync(
            targetDate,
            rawLhpList,
            trungLichIds,
            danhSachTrungLich,
            isLecturer: true,
            currentStudentId: null,
            currentMonday: monday,
            currentSunday: sunday);

        // Danh sách thẻ theo từng thứ trong tuần (định vị theo timeline giờ học UTC)
        var timelineCardsByDay = BuildTimelineCardsByDay(cacThuConfig, tatCaCards);

        // Danh sách lớp học phần hiển thị ở bảng phía trên chuẩn UTC
        var dsLopHocPhan = rawLhpList.Select(l => new LopHocPhanItemDto
        {
            Id = l.Id,
            MaLopHocPhan = l.MaLopHocPhan,
            TenLopHocPhan = l.TenLopHocPhan,
            TenMon = l.MonHoc?.TenMon ?? l.TenLopHocPhan,
            MaMon = l.MonHoc?.MaMon ?? "",
            SoTinChi = l.MonHoc?.SoTinChi ?? 0,
            HinhThucHoc = "Thực hành",
            NgayBatDau = selectedHocKy?.NgayBatDau.ToString("dd/MM/yyyy") ?? "10/08/2026",
            NgayKetThuc = selectedHocKy?.NgayKetThuc.ToString("dd/MM/yyyy") ?? "22/11/2026",
            GhiChu = string.IsNullOrWhiteSpace(l.GhiChu) ? "Chính khóa" : l.GhiChu,
            TenGiangVien = giangVien.HoTen,
            PhongHoc = l.PhongHoc ?? "Chưa xếp phòng",
            ThoiGianHoc = !string.IsNullOrWhiteSpace(l.Thu) ? $"{l.Thu}, Tiết {l.TietBatDau}–{l.TietBatDau + l.SoTiet - 1}" : "Chưa xếp lịch",
            LopPhuTrach = l.DangKyHocPhans.Select(dk => dk.SinhVien?.Lop?.TenLop).Where(s => !string.IsNullOrEmpty(s)).Distinct().FirstOrDefault() ?? "Đại trà",
            SiSo = l.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED")
        }).ToList();

        // 7. Thống kê số liệu
        var tongMonPhuTrach = rawLhpList.Select(l => l.MonHocId).Distinct().Count();
        var tongLopPhuTrach = rawLhpList.Count;
        var tongBuoiDayTrongTuan = tatCaCards.Count;
        var tongSinhVien = rawLhpList.SelectMany(l => l.DangKyHocPhans.Where(dk => dk.TrangThai == "ENROLLED")).Select(dk => dk.SinhVienId).Distinct().Count();

        return new GiangVienLichGiangDayViewModel
        {
            GiangVien = giangVien,
            HocKyHienTai = selectedHocKy,
            DanhSachHocKy = allHocKies,
            NamHocHienTai = selectedNamHoc,
            DanhSachNamHoc = allNamHocs,
            DanhSachMonHoc = dsMonHocCuaGv,
            SelectedNamHocId = selectedNamHoc?.NamHocId,
            SelectedHocKyId = selectedHocKy?.HocKyId,
            SelectedMonHocId = monHocId,
            ViewMode = string.Equals(viewMode, "month", StringComparison.OrdinalIgnoreCase) ? "month" : "week",
            NgayDauTuan = monday,
            NgayCuoiTuan = sunday,
            ChuoiTuanHienThi = $"Tuần {monday:dd/MM/yyyy} – {sunday:dd/MM/yyyy}",
            DanhSachLopHocPhan = dsLopHocPhan,
            MiniCalendar = miniCalendar,
            TimelineCardsByDay = timelineCardsByDay,
            ThangView = thangView,
            TongMonPhuTrach = tongMonPhuTrach,
            TongLopPhuTrach = tongLopPhuTrach,
            TongBuoiDayTrongTuan = tongBuoiDayTrongTuan,
            TongSinhVienPhuTrach = tongSinhVien,
            CacNgayTrongTuan = cacThuConfig,
            DanhSachTrungLich = danhSachTrungLich,
            MaTranTuan = maTranTuan,
            MaTranThoiKhoaBieu = maTranCu,
            TatCaBuoiDay = tatCaCards.OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau).ToList()
        };
    }

    public async Task<SinhVienThoiKhoaBieuViewModel> GetSinhVienThoiKhoaBieuAsync(
        int sinhVienId,
        int? hocKyId = null,
        int? namHocId = null,
        int? monHocId = null,
        string? ngay = null,
        string? viewMode = "week")
    {
        var sinhVien = await _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop).ThenInclude(l => l!.Nganh)
            .FirstOrDefaultAsync(s => s.Id == sinhVienId)
            ?? throw new ArgumentException("Không tìm thấy thông tin sinh viên");

        // 1. Danh sách Học kỳ & Năm học để lọc
        var allNamHocs = await _db.NamHocs.AsNoTracking().OrderByDescending(n => n.NgayBatDau).ToListAsync();
        var allHocKies = await _db.HocKies.AsNoTracking().Include(h => h.NamHoc).OrderByDescending(h => h.NgayBatDau).ToListAsync();

        HocKy? selectedHocKy = null;
        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            selectedHocKy = allHocKies.FirstOrDefault(h => h.HocKyId == hocKyId.Value);
        }
        else if (namHocId.HasValue && namHocId.Value > 0)
        {
            selectedHocKy = allHocKies.FirstOrDefault(h => h.NamHocId == namHocId.Value);
        }

        if (selectedHocKy == null)
        {
            var todayOnly = DateOnly.FromDateTime(DateTime.Today);
            selectedHocKy = allHocKies.FirstOrDefault(h => h.TrangThai == "DANG_MO" || (h.NgayBatDau <= todayOnly && h.NgayKetThuc >= todayOnly))
                            ?? allHocKies.FirstOrDefault();
        }

        var selectedNamHoc = selectedHocKy != null ? allNamHocs.FirstOrDefault(n => n.NamHocId == selectedHocKy.NamHocId) : allNamHocs.FirstOrDefault();

        // 2. Query lớp học phần đã đăng ký của sinh viên
        var dkQuery = _db.DangKyHocPhans
            .AsNoTracking()
            .Include(dk => dk.LopHocPhan).ThenInclude(l => l.MonHoc)
            .Include(dk => dk.LopHocPhan).ThenInclude(l => l.GiangVien)
            .Include(dk => dk.LopHocPhan).ThenInclude(l => l.HocKy).ThenInclude(h => h!.NamHoc)
            .Where(dk => dk.SinhVienId == sinhVienId && dk.TrangThai == "ENROLLED")
            .AsQueryable();

        if (selectedHocKy != null)
        {
            dkQuery = dkQuery.Where(dk => dk.LopHocPhan.HocKyId == selectedHocKy.HocKyId);
        }

        if (monHocId.HasValue && monHocId.Value > 0)
        {
            dkQuery = dkQuery.Where(dk => dk.LopHocPhan.MonHocId == monHocId.Value);
        }

        var registeredList = await dkQuery.ToListAsync();
        var rawLhpList = registeredList.Select(dk => dk.LopHocPhan).ToList();

        var dsMonHocDaDangKy = registeredList
            .Where(dk => dk.LopHocPhan.MonHoc != null)
            .Select(dk => dk.LopHocPhan.MonHoc!)
            .Distinct()
            .OrderBy(m => m.TenMon)
            .ToList();

        // 3. Tính toán thời gian tuần & tháng
        var (monday, sunday, targetDate) = CalculateWeekBounds(ngay);

        var dateMap = BuildDateMap(monday);
        var cacThuConfig = BuildWeekDays(monday);

        // 4. Kiểm tra trùng lịch sinh viên
        var (trungLichIds, danhSachTrungLich) = CheckStudentConflicts(rawLhpList);

        // 5. Build Cards & Ma trận tuần
        var (maTranTuan, maTranCu, tatCaCards) = await BuildTimetableCardsAsync(
            rawLhpList,
            cacThuConfig,
            dateMap,
            trungLichIds,
            danhSachTrungLich,
            isLecturer: false,
            currentStudentId: sinhVienId);

        // 6. Xây dựng Lịch tháng nếu viewMode == "month"
        ThangCalendarViewModel? thangView = null;
        if (string.Equals(viewMode, "month", StringComparison.OrdinalIgnoreCase))
        {
            thangView = await BuildMonthCalendarAsync(
                targetDate,
                rawLhpList,
                trungLichIds,
                danhSachTrungLich,
                isLecturer: false,
                currentStudentId: sinhVienId,
                currentMonday: monday,
                currentSunday: sunday);
        }

        // Mini Calendar Widget chuẩn UTC
        var miniCalendar = await BuildMonthCalendarAsync(
            targetDate,
            rawLhpList,
            trungLichIds,
            danhSachTrungLich,
            isLecturer: false,
            currentStudentId: sinhVienId,
            currentMonday: monday,
            currentSunday: sunday);

        // Danh sách thẻ theo từng thứ trong tuần (định vị theo timeline giờ học UTC)
        var timelineCardsByDay = BuildTimelineCardsByDay(cacThuConfig, tatCaCards);

        // Danh sách lớp học phần sinh viên đã đăng ký hiển thị bảng phía trên chuẩn UTC
        var dsLopHocPhan = registeredList.Select(dk => new LopHocPhanItemDto
        {
            Id = dk.LopHocPhanId,
            MaLopHocPhan = dk.LopHocPhan.MaLopHocPhan,
            TenLopHocPhan = dk.LopHocPhan.TenLopHocPhan,
            TenMon = dk.LopHocPhan.MonHoc?.TenMon ?? dk.LopHocPhan.TenLopHocPhan,
            MaMon = dk.LopHocPhan.MonHoc?.MaMon ?? "",
            SoTinChi = dk.LopHocPhan.MonHoc?.SoTinChi ?? 0,
            HinhThucHoc = "Thực hành",
            NgayBatDau = selectedHocKy?.NgayBatDau.ToString("dd/MM/yyyy") ?? "10/08/2026",
            NgayKetThuc = selectedHocKy?.NgayKetThuc.ToString("dd/MM/yyyy") ?? "22/11/2026",
            GhiChu = string.IsNullOrWhiteSpace(dk.LopHocPhan.GhiChu) ? "Chính khóa" : dk.LopHocPhan.GhiChu,
            TenGiangVien = dk.LopHocPhan.GiangVien?.HoTen ?? "Chưa phân công",
            PhongHoc = dk.LopHocPhan.PhongHoc ?? "Chưa xếp phòng",
            ThoiGianHoc = !string.IsNullOrWhiteSpace(dk.LopHocPhan.Thu) ? $"{dk.LopHocPhan.Thu}, Tiết {dk.LopHocPhan.TietBatDau}–{dk.LopHocPhan.TietBatDau + dk.LopHocPhan.SoTiet - 1}" : "Chưa xếp lịch",
            SiSo = dk.LopHocPhan.DangKyHocPhans?.Count ?? 0
        }).ToList();

        var tongLopHocPhan = registeredList.Count;
        var tongBuoiHocTrongTuan = tatCaCards.Count;
        var tongTinChi = registeredList.Sum(dk => dk.LopHocPhan.MonHoc?.SoTinChi ?? 0);

        return new SinhVienThoiKhoaBieuViewModel
        {
            SinhVien = sinhVien,
            HocKyHienTai = selectedHocKy,
            DanhSachHocKy = allHocKies,
            NamHocHienTai = selectedNamHoc,
            DanhSachNamHoc = allNamHocs,
            DanhSachMonHoc = dsMonHocDaDangKy,
            SelectedNamHocId = selectedNamHoc?.NamHocId,
            SelectedHocKyId = selectedHocKy?.HocKyId,
            SelectedMonHocId = monHocId,
            ViewMode = string.Equals(viewMode, "month", StringComparison.OrdinalIgnoreCase) ? "month" : "week",
            NgayDauTuan = monday,
            NgayCuoiTuan = sunday,
            ChuoiTuanHienThi = $"Tuần {monday:dd/MM/yyyy} – {sunday:dd/MM/yyyy}",
            DanhSachLopHocPhan = dsLopHocPhan,
            MiniCalendar = miniCalendar,
            TimelineCardsByDay = timelineCardsByDay,
            ThangView = thangView,
            TongLopHocPhan = tongLopHocPhan,
            TongBuoiHocTrongTuan = tongBuoiHocTrongTuan,
            TongTinChiDangKy = tongTinChi,
            CacNgayTrongTuan = cacThuConfig,
            DanhSachTrungLich = danhSachTrungLich,
            MaTranTuan = maTranTuan,
            MaTranThoiKhoaBieu = maTranCu,
            TatCaLopHoc = tatCaCards.OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau).ToList()
        };
    }

    public async Task<ThoiKhoaBieuCardViewModel?> GetChiTietBuoiHocAsync(
        int lopHocPhanId,
        DateTime? ngay = null,
        int? currentSinhVienId = null,
        int? currentGiangVienId = null)
    {
        var lhp = await _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.DangKyHocPhans).ThenInclude(dk => dk.SinhVien).ThenInclude(sv => sv.Lop)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null) return null;

        var ngayHoc = ngay ?? DateTime.Today;
        var tietKetThuc = lhp.TietBatDau + lhp.SoTiet - 1;
        var khungGio = CaHocHelper.GetKhungGio(lhp.TietBatDau, lhp.SoTiet);
        var cacCa = CaHocHelper.GetCacCaGiaoNhau(lhp.TietBatDau, tietKetThuc);
        var caGoc = cacCa.Count > 0 ? cacCa.Min() : 1;
        var (code, text, badge) = CaHocHelper.GetSessionStatus(ngayHoc, lhp.TietBatDau, lhp.SoTiet, lhp.TrangThai);

        var card = new ThoiKhoaBieuCardViewModel
        {
            LopHocPhanId = lhp.Id,
            MaLopHocPhan = lhp.MaLopHocPhan,
            TenLopHocPhan = lhp.TenLopHocPhan,
            NhomLop = lhp.Nhom,
            MonHocId = lhp.MonHocId,
            MaMon = lhp.MonHoc?.MaMon ?? "",
            TenMon = lhp.MonHoc?.TenMon ?? lhp.TenLopHocPhan,
            SoTinChi = lhp.MonHoc?.SoTinChi ?? 0,
            GiangVienId = lhp.GiangVienId,
            GiangVien = lhp.GiangVien?.HoTen ?? "Chưa phân công",
            EmailGiangVien = lhp.GiangVien?.Email,
            SoDienThoaiGiangVien = lhp.GiangVien?.SoDienThoai,
            Thu = lhp.Thu ?? "Thứ 2",
            NgayHoc = ngayHoc,
            TietBatDau = lhp.TietBatDau,
            SoTiet = lhp.SoTiet,
            KhungGio = khungGio,
            PhongHoc = lhp.PhongHoc ?? "Chưa xếp phòng",
            CaHoc = caGoc,
            TenCaHoc = CaHocHelper.GetTenCa(caGoc),
            CaHocGoc = caGoc,
            LaCaKeoDai = false,
            StatusCode = code,
            StatusText = text,
            BadgeClass = badge,
            ColorClass = CaHocHelper.GetCourseColorClass(lhp.MonHoc?.MaMon),
            SiSo = lhp.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED"),
            SiSoToiDa = lhp.SiSoToiDa,
            GhiChu = lhp.GhiChu
        };

        // Lấy danh sách điểm danh ngày đó
        var dateOnly = ngayHoc.Date;
        var dsDiemDanh = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId == lhp.Id && d.NgayDiemDanh.Date == dateOnly)
            .ToListAsync();

        card.DaDiemDanh = dsDiemDanh.Count > 0;

        // Nếu là sinh viên
        if (currentSinhVienId.HasValue)
        {
            var myDd = dsDiemDanh.FirstOrDefault(d => d.SinhVienId == currentSinhVienId.Value);
            card.TrangThaiDiemDanhCuaSinhVien = myDd?.TrangThai ?? (card.StatusCode == "DA_HOAN_THANH" ? "Chưa điểm danh" : "Chưa đến giờ");
        }

        // Roster sinh viên
        foreach (var dk in lhp.DangKyHocPhans.Where(d => d.TrangThai == "ENROLLED"))
        {
            var sv = dk.SinhVien;
            var dd = dsDiemDanh.FirstOrDefault(d => d.SinhVienId == sv.Id);
            card.DanhSachSinhVien.Add(new SinhVienSimpleItemDto
            {
                SinhVienId = sv.Id,
                MaSinhVien = sv.MaSinhVien,
                HoTen = sv.HoTen,
                TenLopHanhChinh = sv.Lop?.TenLop,
                Email = sv.Email,
                SoDienThoai = sv.SoDienThoai,
                TrangThaiDiemDanh = dd?.TrangThai ?? "Chưa điểm danh"
            });
        }

        return card;
    }

    // =========================================================================
    // PRIVATE HELPER METHODS
    // =========================================================================

    private static (DateTime monday, DateTime sunday, DateTime targetDate) CalculateWeekBounds(string? ngay)
    {
        DateTime targetDate = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(ngay) && DateTime.TryParseExact(ngay, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            targetDate = parsed;
        }

        int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = targetDate.AddDays(-1 * diff).Date;
        var sunday = monday.AddDays(6).Date;

        return (monday, sunday, targetDate);
    }

    private static Dictionary<string, DateTime> BuildDateMap(DateTime monday)
    {
        return new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase)
        {
            { "Thứ 2", monday },
            { "Thứ hai", monday },
            { "Thứ 3", monday.AddDays(1) },
            { "Thứ ba", monday.AddDays(1) },
            { "Thứ 4", monday.AddDays(2) },
            { "Thứ tư", monday.AddDays(2) },
            { "Thứ 5", monday.AddDays(3) },
            { "Thứ năm", monday.AddDays(3) },
            { "Thứ 6", monday.AddDays(4) },
            { "Thứ sáu", monday.AddDays(4) },
            { "Thứ 7", monday.AddDays(5) },
            { "Thứ bảy", monday.AddDays(5) },
            { "Chủ nhật", monday.AddDays(6) },
            { "CN", monday.AddDays(6) }
        };
    }

    private static List<NgayTrongTuanViewModel> BuildWeekDays(DateTime monday)
    {
        var today = DateTime.Today;
        return new List<NgayTrongTuanViewModel>
        {
            new NgayTrongTuanViewModel { Thu = "Thứ 2", TenRutGon = "T2", Ngay = monday, LaHomNay = monday.Date == today },
            new NgayTrongTuanViewModel { Thu = "Thứ 3", TenRutGon = "T3", Ngay = monday.AddDays(1), LaHomNay = monday.AddDays(1).Date == today },
            new NgayTrongTuanViewModel { Thu = "Thứ 4", TenRutGon = "T4", Ngay = monday.AddDays(2), LaHomNay = monday.AddDays(2).Date == today },
            new NgayTrongTuanViewModel { Thu = "Thứ 5", TenRutGon = "T5", Ngay = monday.AddDays(3), LaHomNay = monday.AddDays(3).Date == today },
            new NgayTrongTuanViewModel { Thu = "Thứ 6", TenRutGon = "T6", Ngay = monday.AddDays(4), LaHomNay = monday.AddDays(4).Date == today },
            new NgayTrongTuanViewModel { Thu = "Thứ 7", TenRutGon = "T7", Ngay = monday.AddDays(5), LaHomNay = monday.AddDays(5).Date == today },
            new NgayTrongTuanViewModel { Thu = "Chủ nhật", TenRutGon = "CN", Ngay = monday.AddDays(6), LaHomNay = monday.AddDays(6).Date == today }
        };
    }

    private static (HashSet<int> conflictIds, List<TrungLichItemViewModel> list) CheckLecturerConflicts(List<LopHocPhan> lhpList)
    {
        var conflictIds = new HashSet<int>();
        var list = new List<TrungLichItemViewModel>();

        for (int i = 0; i < lhpList.Count; i++)
        {
            for (int j = i + 1; j < lhpList.Count; j++)
            {
                var a = lhpList[i];
                var b = lhpList[j];

                if (string.IsNullOrWhiteSpace(a.Thu) || string.IsNullOrWhiteSpace(b.Thu)) continue;
                if (!string.Equals(a.Thu.Trim(), b.Thu.Trim(), StringComparison.OrdinalIgnoreCase)) continue;

                var aEnd = a.TietBatDau + a.SoTiet - 1;
                var bEnd = b.TietBatDau + b.SoTiet - 1;

                // Giao nhau về tiết học
                if (Math.Max(a.TietBatDau, b.TietBatDau) <= Math.Min(aEnd, bEnd))
                {
                    conflictIds.Add(a.Id);
                    conflictIds.Add(b.Id);

                    int overlapStart = Math.Max(a.TietBatDau, b.TietBatDau);
                    int overlapEnd = Math.Min(aEnd, bEnd);
                    string tietTrung = overlapStart == overlapEnd ? $"Tiết {overlapStart}" : $"Tiết {overlapStart}–{overlapEnd}";

                    list.Add(new TrungLichItemViewModel
                    {
                        Thu = a.Thu,
                        LopA = a.MaLopHocPhan,
                        MonA = a.MonHoc?.TenMon ?? a.TenLopHocPhan,
                        TietA = $"Tiết {a.TietBatDau}–{aEnd}",
                        LopB = b.MaLopHocPhan,
                        MonB = b.MonHoc?.TenMon ?? b.TenLopHocPhan,
                        TietB = $"Tiết {b.TietBatDau}–{bEnd}",
                        TietTrung = tietTrung,
                        LoaiTrung = "TRUNG_TIET"
                    });
                }
            }
        }

        return (conflictIds, list);
    }

    private static (HashSet<int> conflictIds, List<TrungLichItemViewModel> list) CheckStudentConflicts(List<LopHocPhan> lhpList)
    {
        var conflictIds = new HashSet<int>();
        var list = new List<TrungLichItemViewModel>();

        for (int i = 0; i < lhpList.Count; i++)
        {
            for (int j = i + 1; j < lhpList.Count; j++)
            {
                var a = lhpList[i];
                var b = lhpList[j];

                if (string.IsNullOrWhiteSpace(a.Thu) || string.IsNullOrWhiteSpace(b.Thu)) continue;
                if (!string.Equals(a.Thu.Trim(), b.Thu.Trim(), StringComparison.OrdinalIgnoreCase)) continue;

                var aEnd = a.TietBatDau + a.SoTiet - 1;
                var bEnd = b.TietBatDau + b.SoTiet - 1;

                if (Math.Max(a.TietBatDau, b.TietBatDau) <= Math.Min(aEnd, bEnd))
                {
                    conflictIds.Add(a.Id);
                    conflictIds.Add(b.Id);

                    int overlapStart = Math.Max(a.TietBatDau, b.TietBatDau);
                    int overlapEnd = Math.Min(aEnd, bEnd);
                    string tietTrung = overlapStart == overlapEnd ? $"Tiết {overlapStart}" : $"Tiết {overlapStart}–{overlapEnd}";

                    list.Add(new TrungLichItemViewModel
                    {
                        Thu = a.Thu,
                        LopA = a.MaLopHocPhan,
                        MonA = a.MonHoc?.TenMon ?? a.TenLopHocPhan,
                        TietA = $"Tiết {a.TietBatDau}–{aEnd}",
                        LopB = b.MaLopHocPhan,
                        MonB = b.MonHoc?.TenMon ?? b.TenLopHocPhan,
                        TietB = $"Tiết {b.TietBatDau}–{bEnd}",
                        TietTrung = tietTrung,
                        LoaiTrung = "TRUNG_TIET"
                    });
                }
            }
        }

        return (conflictIds, list);
    }

    private async Task<(Dictionary<string, ThoiKhoaBieuCellViewModel> maTranTuan, Dictionary<string, List<ThoiKhoaBieuCardViewModel>> maTranCu, List<ThoiKhoaBieuCardViewModel> tatCaCards)> BuildTimetableCardsAsync(
        List<LopHocPhan> lhpList,
        List<NgayTrongTuanViewModel> cacThuConfig,
        Dictionary<string, DateTime> dateMap,
        HashSet<int> trungLichIds,
        List<TrungLichItemViewModel> danhSachTrungLich,
        bool isLecturer,
        int? currentStudentId)
    {
        // 1. Khởi tạo ma trận rỗng cho 4 ca x 7 thứ
        var maTranTuan = new Dictionary<string, ThoiKhoaBieuCellViewModel>(StringComparer.OrdinalIgnoreCase);
        var maTranCu = new Dictionary<string, List<ThoiKhoaBieuCardViewModel>>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in cacThuConfig)
        {
            for (int ca = 1; ca <= 4; ca++)
            {
                var key = $"{c.Thu}_Ca{ca}";
                maTranTuan[key] = new ThoiKhoaBieuCellViewModel
                {
                    Thu = c.Thu,
                    Ca = ca,
                    RowSpan = 1,
                    IsCoveredByRowSpan = false,
                    DanhSachLop = new List<ThoiKhoaBieuCardViewModel>()
                };
                maTranCu[key] = new List<ThoiKhoaBieuCardViewModel>();
            }
        }

        var tatCaCards = new List<ThoiKhoaBieuCardViewModel>();

        // Cache danh sách điểm danh tuần hiện tại
        var lhpIds = lhpList.Select(l => l.Id).ToList();
        var monday = cacThuConfig.First().Ngay.Date;
        var sunday = cacThuConfig.Last().Ngay.Date;

        var diemDanhList = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId.HasValue && lhpIds.Contains(d.LopHocPhanId.Value) && d.NgayDiemDanh.Date >= monday && d.NgayDiemDanh.Date <= sunday)
            .ToListAsync();

        foreach (var lhp in lhpList)
        {
            if (string.IsNullOrWhiteSpace(lhp.Thu)) continue;
            var thuChuan = lhp.Thu.Trim();
            DateTime? ngayHoc = dateMap.ContainsKey(thuChuan) ? dateMap[thuChuan] : null;

            var tietKetThuc = lhp.TietBatDau + lhp.SoTiet - 1;
            var khungGio = CaHocHelper.GetKhungGio(lhp.TietBatDau, lhp.SoTiet);

            // Xác định ca bắt đầu (startCa) và ca kết thúc (endCa) theo chuẩn đào tạo UTC:
            // Ca 1: Tiết 1-2; Ca 2: Tiết 3-4; Ca 3: Tiết 5-6; Ca 4: Tiết 7-8
            int startCa;
            if (lhp.TietBatDau <= 2) startCa = 1;
            else if (lhp.TietBatDau <= 4) startCa = 2;
            else if (lhp.TietBatDau <= 6) startCa = 3;
            else startCa = 4;

            int endCa;
            if (tietKetThuc <= 2) endCa = 1;
            else if (tietKetThuc <= 4) endCa = 2;
            else if (tietKetThuc <= 6) endCa = 3;
            else endCa = 4;

            // Xử lý các buổi học kéo dài qua nhiều ca (ví dụ Tiết 1-4 là Ca 12 & Ca 34, Tiết 5-8 là Ca 56 & Ca 78)
            int spanCount = 1;
            string? ghiChuGiaiLao = null;
            string tenCaHienThi = CaHocHelper.GetTenCa(startCa);

            if (startCa == 1 && endCa >= 2)
            {
                spanCount = 2;
                tenCaHienThi = "Ca 12 – 34";
                ghiChuGiaiLao = "Nghỉ giải lao: 09:30 – 09:35";
            }
            else if (startCa == 3 && endCa >= 4)
            {
                spanCount = 2;
                tenCaHienThi = "Ca 56 – 78";
                ghiChuGiaiLao = "Nghỉ giải lao: 15:10 – 15:35";
            }

            var isConflict = trungLichIds.Contains(lhp.Id);
            string? reason = null;
            if (isConflict)
            {
                var tr = danhSachTrungLich.FirstOrDefault(x => x.LopA == lhp.MaLopHocPhan || x.LopB == lhp.MaLopHocPhan);
                if (tr != null)
                {
                    reason = $"Trùng {tr.TietTrung} với {(tr.LopA == lhp.MaLopHocPhan ? tr.LopB : tr.LopA)}";
                }
            }

            var (statusCode, statusText, badgeClass) = ngayHoc.HasValue
                ? CaHocHelper.GetSessionStatus(ngayHoc.Value, lhp.TietBatDau, lhp.SoTiet, lhp.TrangThai)
                : ("SAP_DIEN_RA", "Sắp diễn ra", "badge-status-upcoming");

            var colorClass = CaHocHelper.GetCourseColorClass(lhp.MonHoc?.MaMon);

            bool daDiemDanh = false;
            string trangThaiDiemDanhSV = "Chưa điểm danh";
            if (ngayHoc.HasValue)
            {
                var ddOnDate = diemDanhList.Where(d => d.LopHocPhanId == lhp.Id && d.NgayDiemDanh.Date == ngayHoc.Value.Date).ToList();
                daDiemDanh = ddOnDate.Count > 0;
                if (currentStudentId.HasValue)
                {
                    var myRecord = ddOnDate.FirstOrDefault(d => d.SinhVienId == currentStudentId.Value);
                    trangThaiDiemDanhSV = myRecord?.TrangThai ?? (statusCode == "DA_HOAN_THANH" ? "Chưa điểm danh" : "Chưa đến giờ");
                }
            }

            var siSoHienTai = lhp.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED");

            // Tạo đúng 1 card duy nhất cho buổi học này! Tuyệt đối không nhân bản thẻ
            var singleCard = new ThoiKhoaBieuCardViewModel
            {
                LopHocPhanId = lhp.Id,
                MaLopHocPhan = lhp.MaLopHocPhan,
                TenLopHocPhan = lhp.TenLopHocPhan,
                NhomLop = lhp.Nhom,
                MonHocId = lhp.MonHocId,
                MaMon = lhp.MonHoc?.MaMon ?? "",
                TenMon = lhp.MonHoc?.TenMon ?? lhp.TenLopHocPhan,
                SoTinChi = lhp.MonHoc?.SoTinChi ?? 0,
                GiangVienId = lhp.GiangVienId,
                GiangVien = lhp.GiangVien?.HoTen ?? "Chưa phân công",
                EmailGiangVien = lhp.GiangVien?.Email,
                SoDienThoaiGiangVien = lhp.GiangVien?.SoDienThoai,
                Thu = thuChuan,
                NgayHoc = ngayHoc,
                TietBatDau = lhp.TietBatDau,
                SoTiet = lhp.SoTiet,
                KhungGio = khungGio,
                PhongHoc = lhp.PhongHoc ?? "Chưa xếp phòng",
                CaHoc = startCa,
                TenCaHoc = tenCaHienThi,
                CaHocGoc = startCa,
                LaCaKeoDai = spanCount > 1,
                RowSpan = spanCount,
                GhiChuGiaiLao = ghiChuGiaiLao,
                StatusCode = statusCode,
                StatusText = statusText,
                BadgeClass = badgeClass,
                ColorClass = colorClass,
                BiTrungLich = isConflict,
                LyDoTrungLich = reason,
                GhiChu = lhp.GhiChu,
                SiSo = siSoHienTai,
                SiSoToiDa = lhp.SiSoToiDa,
                DaDiemDanh = daDiemDanh,
                TrangThaiDiemDanhCuaSinhVien = trangThaiDiemDanhSV
            };

            tatCaCards.Add(singleCard);

            // Gán card vào ô bắt đầu của ma trận tuần
            var startKey = $"{thuChuan}_Ca{startCa}";
            if (maTranTuan.ContainsKey(startKey))
            {
                var cell = maTranTuan[startKey];
                cell.DanhSachLop.Add(singleCard);
                if (spanCount > cell.RowSpan)
                {
                    cell.RowSpan = spanCount;
                }
            }
            if (maTranCu.ContainsKey(startKey))
            {
                maTranCu[startKey].Add(singleCard);
            }

            // Đánh dấu các ô ca tiếp theo bị chiếm chỗ bởi RowSpan để View bỏ qua render <td>
            for (int nextCa = startCa + 1; nextCa <= endCa; nextCa++)
            {
                var coveredKey = $"{thuChuan}_Ca{nextCa}";
                if (maTranTuan.ContainsKey(coveredKey))
                {
                    maTranTuan[coveredKey].IsCoveredByRowSpan = true;
                }
            }
        }

        // Cập nhật số lớp cho từng ngày (chỉ tính các buổi học độc lập)
        foreach (var ngayObj in cacThuConfig)
        {
            var totalCount = 0;
            for (int ca = 1; ca <= 4; ca++)
            {
                var key = $"{ngayObj.Thu}_Ca{ca}";
                if (maTranTuan.ContainsKey(key) && !maTranTuan[key].IsCoveredByRowSpan)
                {
                    totalCount += maTranTuan[key].DanhSachLop.Count;
                }
            }
            ngayObj.TongSoLop = totalCount;
        }

        return (maTranTuan, maTranCu, tatCaCards);
    }

    private static Dictionary<string, List<ThoiKhoaBieuCardViewModel>> BuildTimelineCardsByDay(
        List<NgayTrongTuanViewModel> cacThuConfig,
        List<ThoiKhoaBieuCardViewModel> tatCaCards)
    {
        var result = new Dictionary<string, List<ThoiKhoaBieuCardViewModel>>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in cacThuConfig)
        {
            var dayCards = tatCaCards
                .Where(x => string.Equals(x.Thu, c.Thu, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.TietBatDau)
                .ToList();

            // Tính vị trí timeline và gán màu nhận diện UTC
            foreach (var card in dayCards)
            {
                var (top, height, startStr, endStr) = CaHocHelper.CalculateTimelinePosition(card.TietBatDau, card.SoTiet);
                card.TimelineTop = top;
                card.TimelineHeight = height;

                var theme = CaHocHelper.GetUtcThemeColor(card.MaMon);
                card.ThemeHeaderColor = theme.HeaderBg;
                card.ThemeBorderColor = theme.BorderColor;
                card.ThemeTagBg = theme.TagBg;
                card.TimelineColStyle = "left: 4px; right: 4px;";
            }

            // Xử lý hiển thị song song nếu có lớp trùng khung giờ trong cùng 1 ngày
            for (int i = 0; i < dayCards.Count; i++)
            {
                var a = dayCards[i];
                var aEnd = a.TietBatDau + a.SoTiet - 1;

                for (int j = i + 1; j < dayCards.Count; j++)
                {
                    var b = dayCards[j];
                    var bEnd = b.TietBatDau + b.SoTiet - 1;

                    if (Math.Max(a.TietBatDau, b.TietBatDau) <= Math.Min(aEnd, bEnd))
                    {
                        a.TimelineColStyle = "left: 2px; width: calc(50% - 3px);";
                        b.TimelineColStyle = "left: calc(50% + 1px); width: calc(50% - 3px);";
                    }
                }
            }

            result[c.Thu] = dayCards;
        }

        return result;
    }

    private async Task<ThangCalendarViewModel> BuildMonthCalendarAsync(
        DateTime targetDate,
        List<LopHocPhan> lhpList,
        HashSet<int> trungLichIds,
        List<TrungLichItemViewModel> danhSachTrungLich,
        bool isLecturer,
        int? currentStudentId,
        DateTime? currentMonday = null,
        DateTime? currentSunday = null)
    {
        var year = targetDate.Year;
        var month = targetDate.Month;

        var firstDayOfMonth = new DateTime(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        // Bắt đầu từ Thứ 2 của tuần chứa ngày đầu tháng
        int startDiff = (7 + (firstDayOfMonth.DayOfWeek - DayOfWeek.Monday)) % 7;
        var calendarStart = firstDayOfMonth.AddDays(-1 * startDiff).Date;

        // Kết thúc vào Chủ nhật của tuần chứa ngày cuối tháng
        int endDiff = (7 + (DayOfWeek.Sunday - lastDayOfMonth.DayOfWeek)) % 7;
        var calendarEnd = lastDayOfMonth.AddDays(endDiff).Date;

        var thangVm = new ThangCalendarViewModel
        {
            Thang = month,
            Nam = year,
            NgayDauThang = firstDayOfMonth,
            NgayCuoiThang = lastDayOfMonth
        };

        var allDays = new List<NgayTrongThangViewModel>();
        var today = DateTime.Today;

        for (var d = calendarStart; d <= calendarEnd; d = d.AddDays(1))
        {
            var dayOfWeek = d.DayOfWeek;
            var thuString = dayOfWeek switch
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

            var dayObj = new NgayTrongThangViewModel
            {
                Ngay = d,
                LaThangHienTai = d.Month == month,
                LaHomNay = d.Date == today,
                LaTuanHienTai = currentMonday.HasValue && currentSunday.HasValue && d.Date >= currentMonday.Value.Date && d.Date <= currentSunday.Value.Date
            };

            // Tìm các lớp học diễn ra vào thứ này
            var matchingLhps = lhpList.Where(l => !string.IsNullOrWhiteSpace(l.Thu) &&
                string.Equals(l.Thu.Trim(), thuString, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var lhp in matchingLhps)
            {
                var tietKetThuc = lhp.TietBatDau + lhp.SoTiet - 1;
                var khungGio = CaHocHelper.GetKhungGio(lhp.TietBatDau, lhp.SoTiet);
                var cacCa = CaHocHelper.GetCacCaGiaoNhau(lhp.TietBatDau, tietKetThuc);
                var caGoc = cacCa.Count > 0 ? cacCa.Min() : 1;
                var (statusCode, statusText, badgeClass) = CaHocHelper.GetSessionStatus(d, lhp.TietBatDau, lhp.SoTiet, lhp.TrangThai);

                dayObj.DanhSachBuoiHoc.Add(new ThoiKhoaBieuCardViewModel
                {
                    LopHocPhanId = lhp.Id,
                    MaLopHocPhan = lhp.MaLopHocPhan,
                    TenLopHocPhan = lhp.TenLopHocPhan,
                    NhomLop = lhp.Nhom,
                    MonHocId = lhp.MonHocId,
                    MaMon = lhp.MonHoc?.MaMon ?? "",
                    TenMon = lhp.MonHoc?.TenMon ?? lhp.TenLopHocPhan,
                    SoTinChi = lhp.MonHoc?.SoTinChi ?? 0,
                    GiangVienId = lhp.GiangVienId,
                    GiangVien = lhp.GiangVien?.HoTen ?? "Chưa phân công",
                    Thu = thuString,
                    NgayHoc = d,
                    TietBatDau = lhp.TietBatDau,
                    SoTiet = lhp.SoTiet,
                    KhungGio = khungGio,
                    PhongHoc = lhp.PhongHoc ?? "Chưa xếp phòng",
                    CaHoc = caGoc,
                    TenCaHoc = CaHocHelper.GetTenCa(caGoc),
                    StatusCode = statusCode,
                    StatusText = statusText,
                    BadgeClass = badgeClass,
                    ColorClass = CaHocHelper.GetCourseColorClass(lhp.MonHoc?.MaMon),
                    BiTrungLich = trungLichIds.Contains(lhp.Id)
                });
            }

            allDays.Add(dayObj);
        }

        // Chia thành từng tuần (mỗi tuần 7 ngày)
        int weekIndex = 1;
        for (int i = 0; i < allDays.Count; i += 7)
        {
            thangVm.CacTuan.Add(new TuanTrongThangViewModel
            {
                TuanSo = weekIndex++,
                CacNgay = allDays.Skip(i).Take(7).ToList()
            });
        }

        return thangVm;
    }
}
