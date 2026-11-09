using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Helpers;
using BTL_StudentMGMT.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BTL_StudentMGMT.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _db;
    private readonly IGiangVienSecurityService _securityService;
    private readonly AttendanceOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        ApplicationDbContext db,
        IGiangVienSecurityService securityService,
        IOptions<AttendanceOptions> options,
        TimeProvider timeProvider,
        ILogger<AttendanceService> logger)
    {
        _db = db;
        _securityService = securityService;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<List<DateTime>> GetValidSessionDatesAsync(int lopHocPhanId)
    {
        var lhp = await _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.HocKy)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null || lhp.HocKy == null) return new List<DateTime>();

        var validDates = new List<DateTime>();
        var startDate = lhp.HocKy.NgayBatDau.ToDateTime(TimeOnly.MinValue);
        var endDate = lhp.HocKy.NgayKetThuc.ToDateTime(TimeOnly.MinValue);

        for (var d = startDate.Date; d <= endDate.Date; d = d.AddDays(1))
        {
            if (ThuHelper.Matches(lhp.Thu, d))
            {
                validDates.Add(d);
            }
        }

        return validDates;
    }

    public async Task<SessionValidationResult> ValidateSessionDateAsync(int lopHocPhanId, DateTime date, ClaimsPrincipal user)
    {
        var lhp = await _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.HocKy)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null)
        {
            return new SessionValidationResult(false, "Không tìm thấy lớp học phần.");
        }

        var targetDate = date.Date;
        var today = _timeProvider.GetLocalNow().Date;

        // Điều kiện 1: Đúng thứ trong tuần của lớp học phần
        if (!ThuHelper.Matches(lhp.Thu, targetDate))
        {
            return new SessionValidationResult(false,
                $"Ngày {targetDate:dd/MM/yyyy} ({ThuHelper.ToThuString(targetDate.DayOfWeek)}) không khớp với lịch học cố định của lớp ({lhp.Thu}).");
        }

        // Điều kiện 2: Nằm trong khoảng thời gian học kỳ
        if (lhp.HocKy != null)
        {
            var startDate = lhp.HocKy.NgayBatDau.ToDateTime(TimeOnly.MinValue);
            var endDate = lhp.HocKy.NgayKetThuc.ToDateTime(TimeOnly.MinValue);
            if (targetDate < startDate || targetDate > endDate)
            {
                return new SessionValidationResult(false,
                    $"Ngày {targetDate:dd/MM/yyyy} nằm ngoài thời gian học kỳ {lhp.HocKy.TenHocKy} ({startDate:dd/MM/yyyy} – {endDate:dd/MM/yyyy}).");
            }
        }

        // Điều kiện 3: Không điểm danh trước cho ngày tương lai
        if (targetDate > today)
        {
            return new SessionValidationResult(false, "Không thể thực hiện điểm danh trước cho buổi học trong tương lai.");
        }

        // Điều kiện 4: Kiểm tra cửa sổ cho phép sửa (EditWindowDays) đối với Giảng viên (Admin / PGV không giới hạn)
        var isAdminOrPgv = user.IsInRole("Admin") || user.IsInRole("PGV");
        if (!isAdminOrPgv)
        {
            var cutoffDate = today.AddDays(-_options.EditWindowDays);
            if (targetDate < cutoffDate)
            {
                return new SessionValidationResult(false,
                    $"Đã quá thời hạn cho phép chỉnh sửa điểm danh (chỉ được sửa trong vòng {_options.EditWindowDays} ngày gần nhất).",
                    IsOutsideEditWindow: true);
            }
        }

        return new SessionValidationResult(true);
    }

    public async Task<DiemDanhFormViewModel?> GetDiemDanhFormAsync(int lopHocPhanId, DateTime date, ClaimsPrincipal user)
    {
        var hasAccess = await _securityService.CanAccessLopHocPhanAsync(user, lopHocPhanId);
        if (!hasAccess)
        {
            _securityService.LogSecurityViolation(user, "GetDiemDanhForm", $"LopHocPhanId={lopHocPhanId}", "Giảng viên truy cập lớp học phần không phụ trách");
            return null;
        }

        var lhp = await _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.MonHoc)
            .Include(l => l.HocKy)
            .Include(l => l.DangKyHocPhans).ThenInclude(dk => dk.SinhVien).ThenInclude(sv => sv.Lop)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null) return null;

        var targetDate = date.Date;
        var validation = await ValidateSessionDateAsync(lopHocPhanId, targetDate, user);

        var validDates = await GetValidSessionDatesAsync(lopHocPhanId);

        // Lấy danh sách các ngày đã điểm danh của lớp này để đánh dấu
        var attendedDates = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId == lopHocPhanId)
            .Select(d => d.NgayDiemDanh)
            .Distinct()
            .ToListAsync();

        var attendedDateSet = attendedDates.Select(d => d.Date).ToHashSet();

        var buoiHocOptions = new List<BuoiHocSelectOption>();
        for (int i = 0; i < validDates.Count; i++)
        {
            var d = validDates[i];
            var daDiemDanh = attendedDateSet.Contains(d.Date);
            buoiHocOptions.Add(new BuoiHocSelectOption
            {
                NgayHoc = d,
                Text = $"Buổi {i + 1} ({d:dd/MM/yyyy} - {ThuHelper.ToThuString(d.DayOfWeek)})" + (daDiemDanh ? " ✓ [Đã điểm danh]" : ""),
                DaDiemDanh = daDiemDanh
            });
        }

        var form = new DiemDanhFormViewModel
        {
            LopHocPhanId = lhp.Id,
            MaLopHocPhan = lhp.MaLopHocPhan,
            TenLopHocPhan = lhp.TenLopHocPhan,
            MonHocId = lhp.MonHocId,
            Thu = lhp.Thu,
            PhongHoc = lhp.PhongHoc,
            KhungGio = $"{CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(lhp.TietBatDau))} ({CaHocHelper.GetKhungGio(lhp.TietBatDau, lhp.SoTiet)}, Tiết {lhp.TietBatDau}–{lhp.TietBatDau + lhp.SoTiet - 1})",
            NgayDiemDanh = targetDate,
            CanEdit = validation.IsValid,
            EditRestrictionReason = validation.ErrorMessage,
            DanhSachBuoiHoc = buoiHocOptions,
            DanhSachSinhVien = new List<DiemDanhItemViewModel>()
        };

        var enrolledStudents = lhp.DangKyHocPhans
            .Where(dk => dk.TrangThai == EnrollmentStatus.Enrolled)
            .Select(dk => dk.SinhVien)
            .OrderBy(sv => sv.MaSinhVien)
            .ToList();

        // Truy vấn điểm danh ngày đó không dùng .Date trên cột
        var nextDay = targetDate.AddDays(1);
        var existingRecords = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId == lopHocPhanId && d.NgayDiemDanh >= targetDate && d.NgayDiemDanh < nextDay)
            .ToListAsync();

        var recordMap = existingRecords.ToDictionary(r => r.SinhVienId);

        foreach (var sv in enrolledStudents)
        {
            recordMap.TryGetValue(sv.Id, out var record);
            form.DanhSachSinhVien.Add(new DiemDanhItemViewModel
            {
                SinhVienId = sv.Id,
                MaSinhVien = sv.MaSinhVien,
                HoTen = sv.HoTen,
                TenLopHanhChinh = sv.Lop?.TenLop ?? "Tự do",
                TrangThai = record != null ? AttendanceStatus.Normalize(record.TrangThai) : null,
                GhiChu = record?.GhiChu
            });
        }

        return form;
    }

    public async Task<SaveAttendanceResult> SaveDiemDanhAsync(DiemDanhFormViewModel model, ClaimsPrincipal user)
    {
        if (!model.LopHocPhanId.HasValue || model.LopHocPhanId.Value <= 0)
        {
            return new SaveAttendanceResult(false, "Vui lòng chọn lớp học phần hợp lệ.");
        }

        var lopHocPhanId = model.LopHocPhanId.Value;
        var hasAccess = await _securityService.CanAccessLopHocPhanAsync(user, lopHocPhanId);
        if (!hasAccess)
        {
            _securityService.LogSecurityViolation(user, "LuuDiemDanh", $"LopHocPhanId={lopHocPhanId}", "Giảng viên cố lưu điểm danh lớp không phụ trách");
            return new SaveAttendanceResult(false, "Bạn không có quyền thực hiện điểm danh cho lớp học phần này.");
        }

        var targetDate = model.NgayDiemDanh.Date;
        var validation = await ValidateSessionDateAsync(lopHocPhanId, targetDate, user);
        if (!validation.IsValid)
        {
            return new SaveAttendanceResult(false, validation.ErrorMessage ?? "Thời gian điểm danh không hợp lệ.");
        }

        if (model.DanhSachSinhVien == null || model.DanhSachSinhVien.Count == 0)
        {
            return new SaveAttendanceResult(false, "Danh sách sinh viên trống, không thể lưu điểm danh.");
        }

        // Bắt buộc tất cả sinh viên phải được chọn trạng thái điểm danh
        var unselectedStudents = model.DanhSachSinhVien.Where(s => string.IsNullOrWhiteSpace(s.TrangThai) || !AttendanceStatus.IsValid(s.TrangThai)).ToList();
        if (unselectedStudents.Count > 0)
        {
            return new SaveAttendanceResult(false, $"Vui lòng chọn trạng thái điểm danh cho toàn bộ sinh viên trước khi lưu ({unselectedStudents.Count} sinh viên chưa có trạng thái).");
        }

        // Kiểm tra danh sách sinh viên hợp lệ (phải ENROLLED trong lớp học phần)
        var enrolledStudentIds = await _db.DangKyHocPhans
            .Where(dk => dk.LopHocPhanId == lopHocPhanId && dk.TrangThai == EnrollmentStatus.Enrolled)
            .Select(dk => dk.SinhVienId)
            .ToHashSetAsync();

        foreach (var item in model.DanhSachSinhVien)
        {
            if (!enrolledStudentIds.Contains(item.SinhVienId))
            {
                _securityService.LogSecurityViolation(user, "LuuDiemDanh", $"LopHocPhanId={lopHocPhanId}", $"Sinh viên ID {item.SinhVienId} không thuộc lớp");
                return new SaveAttendanceResult(false, "Phát hiện sinh viên không thuộc danh sách lớp học phần này.");
            }
        }

        var lhp = await _db.LopHocPhans.FindAsync(lopHocPhanId);
        if (lhp == null)
        {
            return new SaveAttendanceResult(false, "Lớp học phần không tồn tại.");
        }

        // Batch load các bản ghi điểm danh cũ trong ngày này (tránh N+1)
        var nextDay = targetDate.AddDays(1);
        var existingRecords = await _db.DiemDanhs
            .Where(d => d.LopHocPhanId == lopHocPhanId && d.NgayDiemDanh >= targetDate && d.NgayDiemDanh < nextDay)
            .ToListAsync();

        var existingMap = existingRecords.ToDictionary(r => r.SinhVienId);

        int updatedCount = 0;
        foreach (var item in model.DanhSachSinhVien)
        {
            var normalizedStatus = AttendanceStatus.Normalize(item.TrangThai);
            var note = string.IsNullOrWhiteSpace(item.GhiChu) ? null : item.GhiChu.Trim();

            if (existingMap.TryGetValue(item.SinhVienId, out var record))
            {
                record.TrangThai = normalizedStatus;
                record.GhiChu = note;
                record.MonHocId = lhp.MonHocId;
            }
            else
            {
                _db.DiemDanhs.Add(new DiemDanh
                {
                    SinhVienId = item.SinhVienId,
                    LopHocPhanId = lopHocPhanId,
                    MonHocId = lhp.MonHocId,
                    NgayDiemDanh = targetDate,
                    TrangThai = normalizedStatus,
                    GhiChu = note
                });
            }
            updatedCount++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Đã lưu điểm danh thành công cho lớp {MaLhp} ngày {Ngay:dd/MM/yyyy} ({Count} sinh viên).", lhp.MaLopHocPhan, targetDate, updatedCount);

        return new SaveAttendanceResult(true, $"Đã lưu kết quả điểm danh ngày {targetDate:dd/MM/yyyy} thành công cho {updatedCount} sinh viên!", updatedCount);
    }

    public async Task<int> DeleteAttendanceForStudentAsync(int lopHocPhanId, int sinhVienId)
    {
        var records = await _db.DiemDanhs
            .Where(d => d.LopHocPhanId == lopHocPhanId && d.SinhVienId == sinhVienId)
            .ToListAsync();

        if (records.Count > 0)
        {
            _db.DiemDanhs.RemoveRange(records);
            var deleted = await _db.SaveChangesAsync();
            _logger.LogInformation("Đã xóa {Count} bản ghi điểm danh của sinh viên ID {SvId} khỏi lớp học phần ID {LhpId}.", records.Count, sinhVienId, lopHocPhanId);
            return deleted;
        }

        return 0;
    }

    // =========================================================================
    // PHASE 2 IMPLEMENTATIONS: G8, G9, G10, G11, G12
    // =========================================================================

    public async Task<StudentAttendanceReportViewModel> GetStudentAttendanceReportAsync(int sinhVienId, int? hocKyId)
    {
        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sinhVienId);
        if (sv == null)
        {
            throw new ArgumentException("Không tìm thấy thông tin sinh viên.");
        }

        var allHocKies = await _db.HocKies.AsNoTracking().OrderByDescending(h => h.NgayBatDau).ToListAsync();
        int? selectedHocKyId = hocKyId;
        if (!selectedHocKyId.HasValue || selectedHocKyId <= 0)
        {
            var activeHk = allHocKies.FirstOrDefault(h => h.TrangThai == "DANG_MO" || h.TrangThai == "OPEN") ?? allHocKies.FirstOrDefault();
            selectedHocKyId = activeHk?.HocKyId;
        }

        var report = new StudentAttendanceReportViewModel
        {
            SinhVienId = sv.Id,
            MaSinhVien = sv.MaSinhVien,
            HoTen = sv.HoTen,
            SelectedHocKyId = selectedHocKyId,
            DanhSachHocKy = allHocKies.Select(h => new SelectListItem
            {
                Value = h.HocKyId.ToString(),
                Text = h.TenHocKy,
                Selected = h.HocKyId == selectedHocKyId
            }).ToList()
        };

        var dkQuery = _db.DangKyHocPhans
            .AsNoTracking()
            .Include(dk => dk.LopHocPhan).ThenInclude(l => l.MonHoc)
            .Include(dk => dk.LopHocPhan).ThenInclude(l => l.GiangVien)
            .Where(dk => dk.SinhVienId == sinhVienId && dk.TrangThai == EnrollmentStatus.Enrolled);

        if (selectedHocKyId.HasValue)
        {
            dkQuery = dkQuery.Where(dk => dk.LopHocPhan.HocKyId == selectedHocKyId.Value);
        }

        var enrolledClasses = await dkQuery.ToListAsync();
        var lhpIds = enrolledClasses.Select(dk => dk.LopHocPhanId).Distinct().ToList();

        // Lấy tất cả bản ghi điểm danh của các lớp này để tính tổng số buổi của từng lớp
        var classAttendance = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId.HasValue && lhpIds.Contains(d.LopHocPhanId.Value))
            .Select(d => new { d.LopHocPhanId, d.SinhVienId, d.NgayDiemDanh, d.TrangThai, d.GhiChu })
            .ToListAsync();

        var sessionsByClass = classAttendance
            .GroupBy(d => d.LopHocPhanId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.NgayDiemDanh.Date).Distinct().ToList()
            );

        var myRecordsByClass = classAttendance
            .Where(d => d.SinhVienId == sinhVienId)
            .GroupBy(d => d.LopHocPhanId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var dk in enrolledClasses)
        {
            var lhp = dk.LopHocPhan;
            sessionsByClass.TryGetValue(lhp.Id, out var classDates);
            int tongBuoiCuaLop = classDates?.Count ?? 0;

            myRecordsByClass.TryGetValue(lhp.Id, out var myRecords);
            myRecords ??= new();

            var statusList = myRecords.Select(r => r.TrangThai).ToList();
            var calc = AttendanceCalculator.Calculate(statusList, tongBuoiCuaLop, _options);

            var sessionDetails = myRecords
                .OrderBy(r => r.NgayDiemDanh)
                .Select(r => new StudentSessionDetailDto
                {
                    NgayDiemDanh = r.NgayDiemDanh,
                    Thu = ThuHelper.ToThuString(r.NgayDiemDanh.DayOfWeek),
                    TrangThai = AttendanceStatus.Normalize(r.TrangThai),
                    TenTrangThai = AttendanceStatus.ToDisplayName(r.TrangThai),
                    BadgeClass = AttendanceStatus.ToBadgeClass(r.TrangThai),
                    GhiChu = r.GhiChu
                }).ToList();

            report.DanhSachLop.Add(new StudentClassAttendanceItemDto
            {
                LopHocPhanId = lhp.Id,
                MaLopHocPhan = lhp.MaLopHocPhan,
                TenLopHocPhan = lhp.TenLopHocPhan,
                TenMon = lhp.MonHoc?.TenMon ?? lhp.TenLopHocPhan,
                SoTinChi = lhp.MonHoc?.SoTinChi ?? 0,
                TenGiangVien = lhp.GiangVien?.HoTen ?? "Chưa phân công",
                TongBuoi = calc.TongBuoi,
                Present = calc.Present,
                Late = calc.Late,
                Excused = calc.Excused,
                Absent = calc.Absent,
                PhanTramVang = calc.PhanTramVang,
                MucCanhBao = calc.MucCanhBao,
                TenMucCanhBao = calc.TenMucCanhBao,
                BadgeClass = calc.BadgeClass,
                DiemGoiY = calc.DiemGoiY,
                DienGiaiGoiY = calc.DienGiaiGoiY,
                DanhSachBuoiHoc = sessionDetails
            });
        }

        return report;
    }

    public async Task<ClassAttendanceMatrixViewModel> GetClassMatrixAsync(
        int lopHocPhanId,
        DateTime? tuNgay,
        DateTime? denNgay,
        ClaimsPrincipal user)
    {
        var hasAccess = await _securityService.CanAccessLopHocPhanAsync(user, lopHocPhanId);
        if (!hasAccess)
        {
            _securityService.LogSecurityViolation(user, "Attendance.ClassMatrix", $"LopHocPhanId={lopHocPhanId}", "Giảng viên truy cập báo cáo điểm danh lớp không phụ trách");
            throw new UnauthorizedAccessException("Bạn không có quyền xem báo cáo điểm danh của lớp học phần này.");
        }

        var lhp = await _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy)
            .Include(l => l.DangKyHocPhans.Where(dk => dk.TrangThai == EnrollmentStatus.Enrolled))
                .ThenInclude(dk => dk.SinhVien).ThenInclude(sv => sv.Lop)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null)
        {
            throw new ArgumentException("Không tìm thấy lớp học phần.");
        }

        var attendanceQuery = _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId == lopHocPhanId);

        if (tuNgay.HasValue)
        {
            var start = tuNgay.Value.Date;
            attendanceQuery = attendanceQuery.Where(d => d.NgayDiemDanh >= start);
        }
        if (denNgay.HasValue)
        {
            var end = denNgay.Value.Date.AddDays(1);
            attendanceQuery = attendanceQuery.Where(d => d.NgayDiemDanh < end);
        }

        var allRecords = await attendanceQuery.ToListAsync();

        // Danh sách các ngày phân biệt có điểm danh của lớp
        var distinctDates = allRecords
            .Select(r => r.NgayDiemDanh.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        int tongBuoiCuaLop = distinctDates.Count;

        var enrolledStudents = lhp.DangKyHocPhans
            .Where(dk => dk.TrangThai == EnrollmentStatus.Enrolled)
            .Select(dk => dk.SinhVien)
            .OrderBy(sv => sv.MaSinhVien)
            .ToList();

        var recordsByStudent = allRecords
            .GroupBy(r => r.SinhVienId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var studentRows = new List<StudentMatrixRowDto>();
        foreach (var sv in enrolledStudents)
        {
            recordsByStudent.TryGetValue(sv.Id, out var sRecords);
            sRecords ??= new();

            var statusList = sRecords.Select(r => r.TrangThai).ToList();
            var calc = AttendanceCalculator.Calculate(statusList, tongBuoiCuaLop, _options);

            var dateMap = new Dictionary<DateTime, string>();
            foreach (var r in sRecords)
            {
                dateMap[r.NgayDiemDanh.Date] = AttendanceStatus.Normalize(r.TrangThai);
            }

            var row = new StudentMatrixRowDto
            {
                SinhVienId = sv.Id,
                MaSinhVien = sv.MaSinhVien,
                HoTen = sv.HoTen,
                TenLopHanhChinh = sv.Lop?.TenLop ?? "Tự do",
                AttendanceByDate = dateMap,
                TongBuoi = calc.TongBuoi,
                Present = calc.Present,
                Late = calc.Late,
                Excused = calc.Excused,
                Absent = calc.Absent,
                PhanTramVang = calc.PhanTramVang,
                MucCanhBao = calc.MucCanhBao,
                TenMucCanhBao = calc.TenMucCanhBao,
                BadgeClass = calc.BadgeClass,
                DiemGoiY = calc.DiemGoiY,
                DienGiaiGoiY = calc.DienGiaiGoiY
            };
            studentRows.Add(row);
        }

        var nguyCoList = studentRows.Where(r => r.MucCanhBao == AttendanceWarningLevel.NguyCo).ToList();
        var canhBaoList = studentRows.Where(r => r.MucCanhBao == AttendanceWarningLevel.CanhBao).ToList();

        return new ClassAttendanceMatrixViewModel
        {
            LopHocPhanId = lhp.Id,
            MaLopHocPhan = lhp.MaLopHocPhan,
            TenLopHocPhan = lhp.TenLopHocPhan,
            TenMon = lhp.MonHoc?.TenMon ?? lhp.TenLopHocPhan,
            SoTinChi = lhp.MonHoc?.SoTinChi ?? 0,
            TenGiangVien = lhp.GiangVien?.HoTen ?? "Chưa phân công",
            SiSo = enrolledStudents.Count,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongBuoiCuaLop = tongBuoiCuaLop,
            Dates = distinctDates,
            Students = studentRows,
            NguyCoStudents = nguyCoList,
            CanhBaoStudents = canhBaoList,
            CanEdit = true
        };
    }

    public async Task<byte[]> ExportExcelMatrixAsync(
        int lopHocPhanId,
        DateTime? tuNgay,
        DateTime? denNgay,
        ClaimsPrincipal user)
    {
        var matrix = await GetClassMatrixAsync(lopHocPhanId, tuNgay, denNgay, user);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DiemDanh");

        // 1. Tiêu đề chính
        worksheet.Cell(1, 1).Value = $"BẢNG ĐIỂM DANH LỚP HỌC PHẦN: {matrix.TenLopHocPhan.ToUpper()} ({matrix.MaLopHocPhan})";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromArgb(12, 35, 64);

        worksheet.Cell(2, 1).Value = $"Môn học: {matrix.TenMon} ({matrix.SoTinChi} tín chỉ)  |  Giảng viên: {matrix.TenGiangVien}  |  Sĩ số: {matrix.SiSo} sinh viên";
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Cell(2, 1).Style.Font.FontSize = 10;

        string rangeStr = (matrix.TuNgay.HasValue || matrix.DenNgay.HasValue)
            ? $"Khoảng ngày: {(matrix.TuNgay.HasValue ? matrix.TuNgay.Value.ToString("dd/MM/yyyy") : "Đầu kỳ")} - {(matrix.DenNgay.HasValue ? matrix.DenNgay.Value.ToString("dd/MM/yyyy") : "Hiện tại")} | "
            : "";
        worksheet.Cell(3, 1).Value = $"{rangeStr}Tổng số buổi đã điểm danh: {matrix.TongBuoiCuaLop} buổi";
        worksheet.Cell(3, 1).Style.Font.FontSize = 10;

        // 2. Header bảng dữ liệu
        int headerRow = 5;
        worksheet.Cell(headerRow, 1).Value = "STT";
        worksheet.Cell(headerRow, 2).Value = "Mã sinh viên";
        worksheet.Cell(headerRow, 3).Value = "Họ và tên";
        worksheet.Cell(headerRow, 4).Value = "Lớp hành chính";

        int colIdx = 5;
        foreach (var d in matrix.Dates)
        {
            worksheet.Cell(headerRow, colIdx).Value = d.ToString("dd/MM");
            colIdx++;
        }

        int colPresent = colIdx++;
        int colLate = colIdx++;
        int colExcused = colIdx++;
        int colAbsent = colIdx++;
        int colPercent = colIdx++;
        int colWarning = colIdx++;
        int colScore = colIdx++;

        worksheet.Cell(headerRow, colPresent).Value = "Có mặt";
        worksheet.Cell(headerRow, colLate).Value = "Đi muộn";
        worksheet.Cell(headerRow, colExcused).Value = "Có phép";
        worksheet.Cell(headerRow, colAbsent).Value = "Vắng";
        worksheet.Cell(headerRow, colPercent).Value = "% Vắng";
        worksheet.Cell(headerRow, colWarning).Value = "Cảnh báo";
        worksheet.Cell(headerRow, colScore).Value = "Gợi ý ĐCC";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, colScore);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(12, 35, 64);
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        // 3. Data rows
        int curRow = headerRow + 1;
        for (int i = 0; i < matrix.Students.Count; i++)
        {
            var st = matrix.Students[i];
            worksheet.Cell(curRow, 1).Value = i + 1;
            worksheet.Cell(curRow, 2).Value = st.MaSinhVien;
            worksheet.Cell(curRow, 3).Value = st.HoTen;
            worksheet.Cell(curRow, 4).Value = st.TenLopHanhChinh ?? "";

            int dCol = 5;
            foreach (var d in matrix.Dates)
            {
                st.AttendanceByDate.TryGetValue(d.Date, out var code);
                string text = code switch
                {
                    AttendanceStatus.Present => "CM",
                    AttendanceStatus.Late => "DM",
                    AttendanceStatus.Excused => "CP",
                    AttendanceStatus.Absent => "V",
                    _ => "-"
                };
                var cell = worksheet.Cell(curRow, dCol);
                cell.Value = text;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (code == AttendanceStatus.Absent)
                {
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.Style.Font.Bold = true;
                }
                else if (code == AttendanceStatus.Late)
                {
                    cell.Style.Font.FontColor = XLColor.FromArgb(217, 119, 6);
                }
                dCol++;
            }

            worksheet.Cell(curRow, colPresent).Value = st.Present;
            worksheet.Cell(curRow, colLate).Value = st.Late;
            worksheet.Cell(curRow, colExcused).Value = st.Excused;
            worksheet.Cell(curRow, colAbsent).Value = st.Absent;
            worksheet.Cell(curRow, colPercent).Value = $"{st.PhanTramVang}%";
            worksheet.Cell(curRow, colWarning).Value = st.TenMucCanhBao;
            worksheet.Cell(curRow, colScore).Value = st.DiemGoiY.HasValue ? st.DiemGoiY.Value.ToString("0.0") : "-";

            // Highlight theo mức cảnh báo
            if (st.MucCanhBao == AttendanceWarningLevel.NguyCo)
            {
                var rowRange = worksheet.Range(curRow, 1, curRow, colScore);
                rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(254, 226, 226); // Light red
                worksheet.Cell(curRow, colWarning).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(curRow, colWarning).Style.Font.Bold = true;
            }
            else if (st.MucCanhBao == AttendanceWarningLevel.CanhBao)
            {
                var rowRange = worksheet.Range(curRow, 1, curRow, colScore);
                rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(254, 240, 138); // Light yellow
                worksheet.Cell(curRow, colWarning).Style.Font.FontColor = XLColor.FromArgb(161, 98, 7);
                worksheet.Cell(curRow, colWarning).Style.Font.Bold = true;
            }

            curRow++;
        }

        // Định dạng khung viền & căn chỉnh
        var dataRange = worksheet.Range(headerRow, 1, curRow - 1, colScore);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<AttendanceCalculationResult?> GetStudentSuggestionAsync(
        int lopHocPhanId,
        int sinhVienId,
        ClaimsPrincipal user)
    {
        var hasAccess = await _securityService.CanAccessLopHocPhanAsync(user, lopHocPhanId);
        if (!hasAccess)
        {
            _securityService.LogSecurityViolation(user, "Attendance.Suggestion", $"LopHocPhanId={lopHocPhanId}", "Giảng viên cố lấy gợi ý điểm của lớp học phần không phụ trách");
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập lớp học phần này.");
        }

        var isEnrolled = await _db.DangKyHocPhans
            .AnyAsync(dk => dk.LopHocPhanId == lopHocPhanId && dk.SinhVienId == sinhVienId && dk.TrangThai == EnrollmentStatus.Enrolled);

        if (!isEnrolled) return null;

        var distinctDates = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId == lopHocPhanId)
            .Select(d => d.NgayDiemDanh)
            .Distinct()
            .ToListAsync();

        int tongBuoiCuaLop = distinctDates.Count;

        var records = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId == lopHocPhanId && d.SinhVienId == sinhVienId)
            .ToListAsync();

        var statusList = records.Select(r => r.TrangThai).ToList();
        return AttendanceCalculator.Calculate(statusList, tongBuoiCuaLop, _options);
    }

    public async Task<QuanLyDiemDanhIndexViewModel> GetAdminIndexAsync(
        int? hocKyId,
        int? monHocId,
        int? giangVienId,
        string? search,
        int page = 1,
        int pageSize = 15)
    {
        page = Math.Max(1, page);

        var allHocKies = await _db.HocKies.AsNoTracking().OrderByDescending(h => h.NgayBatDau).ToListAsync();
        var allMonHocs = await _db.MonHocs.AsNoTracking().OrderBy(m => m.TenMon).ToListAsync();
        var allGiangViens = await _db.GiangViens.AsNoTracking().OrderBy(g => g.HoTen).ToListAsync();

        var query = _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy)
            .AsQueryable();

        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            query = query.Where(l => l.HocKyId == hocKyId.Value);
        }
        if (monHocId.HasValue && monHocId.Value > 0)
        {
            query = query.Where(l => l.MonHocId == monHocId.Value);
        }
        if (giangVienId.HasValue && giangVienId.Value > 0)
        {
            query = query.Where(l => l.GiangVienId == giangVienId.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim();
            query = query.Where(l => l.MaLopHocPhan.Contains(kw) || l.TenLopHocPhan.Contains(kw));
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page > Math.Max(totalPages, 1)) page = Math.Max(totalPages, 1);

        var pagedLhps = await query
            .OrderByDescending(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var lhpIds = pagedLhps.Select(l => l.Id).ToList();

        // Batch load sĩ số (ENROLLED)
        var sisoDict = await _db.DangKyHocPhans
            .AsNoTracking()
            .Where(dk => lhpIds.Contains(dk.LopHocPhanId) && dk.TrangThai == EnrollmentStatus.Enrolled)
            .GroupBy(dk => dk.LopHocPhanId)
            .Select(g => new { LopHocPhanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LopHocPhanId, x => x.Count);

        // Batch load dữ liệu điểm danh của các lớp này
        var diemDanhData = await _db.DiemDanhs
            .AsNoTracking()
            .Where(d => d.LopHocPhanId.HasValue && lhpIds.Contains(d.LopHocPhanId.Value))
            .Select(d => new { d.LopHocPhanId, d.SinhVienId, d.NgayDiemDanh, d.TrangThai })
            .ToListAsync();

        var items = new List<QuanLyDiemDanhLhpItemDto>();
        foreach (var lhp in pagedLhps)
        {
            var lhpRecords = diemDanhData.Where(d => d.LopHocPhanId == lhp.Id).ToList();
            var distinctSessions = lhpRecords.Select(r => r.NgayDiemDanh.Date).Distinct().ToList();
            int tongBuoi = distinctSessions.Count;

            int nguyCoCount = 0;
            int canhBaoCount = 0;

            if (tongBuoi >= _options.MinSessionsForWarning)
            {
                var studentGroups = lhpRecords.GroupBy(r => r.SinhVienId);
                foreach (var g in studentGroups)
                {
                    var calc = AttendanceCalculator.Calculate(g.Select(x => x.TrangThai), tongBuoi, _options);
                    if (calc.MucCanhBao == AttendanceWarningLevel.NguyCo) nguyCoCount++;
                    else if (calc.MucCanhBao == AttendanceWarningLevel.CanhBao) canhBaoCount++;
                }
            }

            sisoDict.TryGetValue(lhp.Id, out int siSo);

            items.Add(new QuanLyDiemDanhLhpItemDto
            {
                LopHocPhanId = lhp.Id,
                MaLopHocPhan = lhp.MaLopHocPhan,
                TenLopHocPhan = lhp.TenLopHocPhan,
                TenMon = lhp.MonHoc?.TenMon ?? lhp.TenLopHocPhan,
                SoTinChi = lhp.MonHoc?.SoTinChi ?? 0,
                TenGiangVien = lhp.GiangVien?.HoTen ?? "Chưa phân công",
                TenHocKy = lhp.HocKy?.TenHocKy ?? "Chưa xếp kỳ",
                SiSo = siSo,
                TongBuoiDaDiemDanh = tongBuoi,
                SoSinhVienNguyCo = nguyCoCount,
                SoSinhVienCanhBao = canhBaoCount
            });
        }

        return new QuanLyDiemDanhIndexViewModel
        {
            TuKhoa = search,
            HocKyId = hocKyId,
            MonHocId = monHocId,
            GiangVienId = giangVienId,
            Page = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            DanhSachHocKy = allHocKies.Select(h => new SelectListItem
            {
                Value = h.HocKyId.ToString(),
                Text = h.TenHocKy,
                Selected = h.HocKyId == hocKyId
            }).ToList(),
            DanhSachMonHoc = allMonHocs.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.MaMon} - {m.TenMon}",
                Selected = m.Id == monHocId
            }).ToList(),
            DanhSachGiangVien = allGiangViens.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = $"{g.MaGiangVien} - {g.HoTen}",
                Selected = g.Id == giangVienId
            }).ToList(),
            DanhSachLop = items
        };
    }
}
