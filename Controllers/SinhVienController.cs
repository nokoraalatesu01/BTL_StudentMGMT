using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "SinhVien")]
public class SinhVienController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly IPaymentService _paymentService;
    private readonly ITimetableService _timetableService;

    public SinhVienController(
        ApplicationDbContext db, 
        UserManager<ApplicationUser> userManager, 
        IFileUploadService fileUploadService,
        IPaymentService paymentService,
        ITimetableService timetableService)
    {
        _db = db;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _paymentService = paymentService;
        _timetableService = timetableService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await LoadCurrentStudentAsync();
        return model == null ? View("Unlinked") : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatAnhDaiDien(IFormFile? fileAnh)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction(nameof(HoSo));

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return RedirectToAction(nameof(HoSo));

        if (fileAnh != null && fileAnh.Length > 0)
        {
            var avatarPath = await _fileUploadService.UploadAvatarAsync(fileAnh, student.HinhAnh);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                student.HinhAnh = avatarPath;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
            }
            else
            {
                TempData["Error"] = "Định dạng file không hợp lệ (.jpg, .jpeg, .png, .webp) hoặc vượt quá 5MB.";
            }
        }
        else
        {
            TempData["Error"] = "Vui lòng chọn file hình ảnh hợp lệ để tải lên.";
        }

        return RedirectToAction(nameof(HoSo));
    }

    public async Task<IActionResult> HoSo()
    {
        var model = await LoadCurrentStudentAsync();
        return model == null ? View("Unlinked") : View(model);
    }

    public async Task<IActionResult> BangDiem()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return View("Unlinked");
        }

        var student = await _db.SinhViens
            .AsNoTracking()
            .Include(item => item.Lop)
                .ThenInclude(lop => lop!.Nganh)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.MonHoc)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.LopHocPhan)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.HocKy)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (student == null)
        {
            return View("Unlinked");
        }

        var model = new SinhVienDiemViewModel
        {
            MaSinhVien = student.MaSinhVien,
            HoTen = student.HoTen,
            TenLop = student.Lop?.TenLop,
            TenNganh = student.Lop?.Nganh?.TenNganh,
            NienKhoa = student.NienKhoa,
            TrangThai = student.TrangThai,
            HinhAnh = student.HinhAnh,
            Diems = student.Diems
                .OrderBy(diem => diem.MonHoc?.TenMon)
                .ToList()
        };

        return View(model);
    }

    // 4. Màn hình đăng ký học phần (Tín chỉ)
    public async Task<IActionResult> DangKyHocPhan()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return View("Unlinked");

        var student = await _db.SinhViens
            .Include(s => s.Lop)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null) return View("Unlinked");

        // Lấy các lớp học phần đang mở đăng ký
        var lhpList = await _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy)
            .Include(l => l.DangKyHocPhans)
            .Where(l => l.TrangThai == "OPEN")
            .OrderBy(l => l.MonHoc!.TenMon)
            .ThenBy(l => l.Nhom)
            .ToListAsync();

        var registeredList = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
                .ThenInclude(l => l.MonHoc)
            .Include(dk => dk.LopHocPhan)
                .ThenInclude(l => l.GiangVien)
            .Where(dk => dk.SinhVienId == student.Id && dk.TrangThai == "ENROLLED")
            .ToListAsync();

        var registeredLhpIds = registeredList.Select(dk => dk.LopHocPhanId).ToHashSet();

        var vm = new SinhVienDangKyHocPhanViewModel
        {
            SinhVien = student,
            LopHocPhansMo = lhpList.Select(l => new LopHocPhanDangKyItemViewModel
            {
                LopHocPhanId = l.Id,
                MaLopHocPhan = l.MaLopHocPhan,
                TenLopHocPhan = l.TenLopHocPhan,
                MaMon = l.MonHoc?.MaMon ?? "",
                TenMon = l.MonHoc?.TenMon ?? "",
                SoTinChi = l.MonHoc?.SoTinChi ?? 0,
                TenGiangVien = l.GiangVien?.HoTen,
                LichHoc = $"{l.Thu}, Tiết {l.TietBatDau} - {l.TietBatDau + l.SoTiet - 1}",
                PhongHoc = l.PhongHoc,
                SiSoHienTai = l.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED"),
                SiSoToiDa = l.SiSoToiDa,
                DaDangKy = registeredLhpIds.Contains(l.Id)
            }).ToList(),
            LopHocPhanDaDangKy = registeredList.Select(dk => new LopHocPhanDangKyItemViewModel
            {
                LopHocPhanId = dk.LopHocPhanId,
                MaLopHocPhan = dk.LopHocPhan.MaLopHocPhan,
                TenLopHocPhan = dk.LopHocPhan.TenLopHocPhan,
                MaMon = dk.LopHocPhan.MonHoc?.MaMon ?? "",
                TenMon = dk.LopHocPhan.MonHoc?.TenMon ?? "",
                SoTinChi = dk.LopHocPhan.MonHoc?.SoTinChi ?? 0,
                TenGiangVien = dk.LopHocPhan.GiangVien?.HoTen,
                LichHoc = $"{dk.LopHocPhan.Thu}, Tiết {dk.LopHocPhan.TietBatDau} - {dk.LopHocPhan.TietBatDau + dk.LopHocPhan.SoTiet - 1}",
                PhongHoc = dk.LopHocPhan.PhongHoc,
                SiSoHienTai = 0,
                SiSoToiDa = dk.LopHocPhan.SiSoToiDa,
                DaDangKy = true,
                NgayDangKy = dk.NgayDangKy
            }).ToList()
        };

        return View(vm);
    }

    // 5. Xác nhận đăng ký lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanDangKy(int lopHocPhanId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction(nameof(DangKyHocPhan));

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return RedirectToAction(nameof(DangKyHocPhan));

        var lhp = await _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.DangKyHocPhans)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null)
        {
            TempData["Error"] = "Lớp học phần không tồn tại.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        if (lhp.TrangThai != "OPEN")
        {
            TempData["Error"] = "Lớp học phần đã đóng đăng ký hoặc kết thúc.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra đã đăng ký lớp học phần này chưa
        var daDangKy = await _db.DangKyHocPhans.AnyAsync(dk =>
            dk.SinhVienId == student.Id &&
            dk.LopHocPhanId == lopHocPhanId &&
            dk.TrangThai == "ENROLLED");

        if (daDangKy)
        {
            TempData["Error"] = "Bạn đã đăng ký lớp học phần này rồi.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra sĩ số tối đa
        var count = lhp.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED");
        if (count >= lhp.SiSoToiDa)
        {
            TempData["Error"] = "Lớp học phần đã đầy sĩ số (hết chỗ). Vui lòng chọn nhóm lớp khác.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra đã đăng ký nhóm khác của cùng môn học trong cùng học kỳ chưa
        var daDangKyCungMon = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
            .AnyAsync(dk =>
                dk.SinhVienId == student.Id &&
                dk.LopHocPhan.MonHocId == lhp.MonHocId &&
                dk.LopHocPhan.HocKyId == lhp.HocKyId &&
                dk.TrangThai == "ENROLLED");

        if (daDangKyCungMon)
        {
            TempData["Error"] = $"Bạn đã đăng ký một nhóm khác của môn '{lhp.MonHoc?.TenMon}' trong học kỳ này.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra trùng lịch học với các môn đã đăng ký
        var currentEnrollments = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
            .Where(dk => dk.SinhVienId == student.Id && dk.TrangThai == "ENROLLED")
            .Select(dk => dk.LopHocPhan)
            .ToListAsync();

        var tietKetThuc = lhp.TietBatDau + lhp.SoTiet - 1;
        var trungLich = currentEnrollments.FirstOrDefault(other =>
            other.Thu == lhp.Thu &&
            ((lhp.TietBatDau >= other.TietBatDau && lhp.TietBatDau < other.TietBatDau + other.SoTiet) ||
             (tietKetThuc > other.TietBatDau && tietKetThuc <= other.TietBatDau + other.SoTiet)));

        if (trungLich != null)
        {
            TempData["Error"] = $"Trùng lịch học: Lớp này học vào {lhp.Thu} (tiết {lhp.TietBatDau}-{tietKetThuc}), trùng với môn '{trungLich.TenLopHocPhan}'.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Phân tích lịch sử học môn này để tự động xác định loại đăng ký và số lần học
        var previousAttempts = await _db.Diems
            .Where(d => d.SinhVienId == student.Id && d.MonHocId == lhp.MonHocId)
            .OrderByDescending(d => d.LanHoc)
            .ToListAsync();

        string loaiDangKy = "HOC_LAN_DAU";
        int lanHoc = 1;
        string? ghiChuDk = null;

        if (previousAttempts.Count > 0)
        {
            var lastAttempt = previousAttempts.First();
            lanHoc = previousAttempts.Max(d => d.LanHoc) + 1;
            if (lastAttempt.DiemTongKet < 4.0)
            {
                loaiDangKy = "HOC_LAI";
                ghiChuDk = $"Đăng ký học lại do lần {lastAttempt.LanHoc} không đạt ({lastAttempt.DiemTongKet} điểm)";
            }
            else
            {
                loaiDangKy = "HOC_CAI_THIEN";
                ghiChuDk = $"Đăng ký học cải thiện điểm (lần {lastAttempt.LanHoc} đã đạt {lastAttempt.DiemTongKet} điểm)";
            }
        }
        else
        {
            var prevDk = await _db.DangKyHocPhans
                .Include(dk => dk.LopHocPhan)
                .Where(dk => dk.SinhVienId == student.Id && dk.LopHocPhan.MonHocId == lhp.MonHocId && dk.LopHocPhanId != lopHocPhanId)
                .CountAsync();
            if (prevDk > 0)
            {
                lanHoc = prevDk + 1;
                loaiDangKy = "HOC_LAI";
            }
        }

        // Thực hiện ghi danh
        var existingRecord = await _db.DangKyHocPhans.FirstOrDefaultAsync(dk =>
            dk.SinhVienId == student.Id && dk.LopHocPhanId == lopHocPhanId);

        if (existingRecord != null)
        {
            existingRecord.TrangThai = "ENROLLED";
            existingRecord.LoaiDangKy = loaiDangKy;
            existingRecord.LanHoc = lanHoc;
            existingRecord.GhiChu = ghiChuDk;
            existingRecord.NgayDangKy = DateTime.UtcNow;
        }
        else
        {
            _db.DangKyHocPhans.Add(new DangKyHocPhan
            {
                SinhVienId = student.Id,
                LopHocPhanId = lopHocPhanId,
                LoaiDangKy = loaiDangKy,
                LanHoc = lanHoc,
                GhiChu = ghiChuDk,
                NgayDangKy = DateTime.UtcNow,
                TrangThai = "ENROLLED"
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đăng ký thành công lớp học phần '{lhp.TenLopHocPhan}'!";
        return RedirectToAction(nameof(DangKyHocPhan));
    }

    // 6. Hủy đăng ký lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyDangKy(int lopHocPhanId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction(nameof(DangKyHocPhan));

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return RedirectToAction(nameof(DangKyHocPhan));

        var dk = await _db.DangKyHocPhans
            .Include(d => d.LopHocPhan)
            .FirstOrDefaultAsync(d => d.SinhVienId == student.Id && d.LopHocPhanId == lopHocPhanId && d.TrangThai == "ENROLLED");

        if (dk == null)
        {
            TempData["Error"] = "Bản ghi đăng ký không tồn tại.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        if (dk.LopHocPhan.TrangThai != "OPEN")
        {
            TempData["Error"] = "Lớp học phần đã khóa đăng ký, không thể hủy.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        _db.DangKyHocPhans.Remove(dk);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã hủy đăng ký lớp học phần '{dk.LopHocPhan.TenLopHocPhan}'.";
        return RedirectToAction(nameof(DangKyHocPhan));
    }

    // 7. Thời khóa biểu cá nhân của sinh viên
    public async Task<IActionResult> ThoiKhoaBieu(int? hocKyId, int? namHocId, int? monHocId, string? ngay, string? viewMode)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return View("Unlinked");

        // LÀM VIỆC VỚI SESSION: Lưu/phục hồi học kỳ và năm học đang chọn của sinh viên
        if (hocKyId.HasValue) HttpContext.Session.SetInt32("SV_SelectedHocKyId", hocKyId.Value);
        if (namHocId.HasValue) HttpContext.Session.SetInt32("SV_SelectedNamHocId", namHocId.Value);

        hocKyId ??= HttpContext.Session.GetInt32("SV_SelectedHocKyId");
        namHocId ??= HttpContext.Session.GetInt32("SV_SelectedNamHocId");

        var student = await _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop).ThenInclude(l => l!.Nganh)
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return View("Unlinked");

        var viewModel = await _timetableService.GetSinhVienThoiKhoaBieuAsync(
            student.Id,
            hocKyId,
            namHocId,
            monHocId,
            ngay,
            viewMode ?? "week");

        return View(viewModel);
    }

    // 7.1. API Chi tiết buổi học dành cho sinh viên (Xem modal & điểm danh)
    [HttpGet]
    public async Task<IActionResult> ChiTietBuoiHoc(int lopHocPhanId, DateTime? ngay)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var student = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return NotFound();

        var detail = await _timetableService.GetChiTietBuoiHocAsync(
            lopHocPhanId,
            ngay,
            currentSinhVienId: student.Id);

        if (detail == null) return NotFound(new { success = false, message = "Không tìm thấy buổi học" });
        return Json(new { success = true, data = detail });
    }

    private async Task<SinhVienPortalViewModel?> LoadCurrentStudentAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return null;
        }

        var student = await _db.SinhViens
            .AsNoTracking()
            .Include(item => item.Lop)
                .ThenInclude(lop => lop!.Nganh)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.MonHoc)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (student == null)
        {
            return null;
        }

        var classmates = student.Lop == null
            ? (int?)null
            : await _db.SinhViens.CountAsync(item => item.LopId == student.LopId);

        var gpa = student.Gpa;

        return new SinhVienPortalViewModel
        {
            UserName = User.Identity?.Name ?? "",
            HoTen = student.HoTen,
            MaSinhVien = student.MaSinhVien,
            NgaySinh = student.NgaySinh,
            GioiTinh = student.GioiTinh,
            Email = student.Email,
            SoDienThoai = student.SoDienThoai,
            DiaChi = student.DiaChi,
            TenLop = student.Lop?.TenLop,
            MaNganh = student.Lop?.Nganh?.MaNganh,
            TenNganh = student.Lop?.Nganh?.TenNganh,
            TenKhoa = student.Lop?.Nganh?.TenKhoa,
            SiSoLop = classmates,
            NienKhoa = student.NienKhoa,
            TrangThai = student.TrangThai,
            HinhAnh = student.HinhAnh,
            Gpa = gpa,
            XepLoai = student.XepLoai,
            SoMonDaDat = student.SoMonDaDat,
            SoMon = student.TongSoMonHoc
        };
    }

    // 8. Quản lý & Thanh toán Học phí trực tuyến VietQR
    public async Task<IActionResult> HocPhi(int? hocKyId, string? trangThai)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return View("Unlinked");

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return View("Unlinked");

        var model = await _paymentService.GetSinhVienHocPhiAsync(student.Id, hocKyId, trangThai);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> TaoGiaoDichHocPhi([FromBody] TaoPaymentRequest request)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return Unauthorized(new { success = false, message = "Không tìm thấy sinh viên." });

        var result = await _paymentService.CreatePaymentAsync(request.HoaDonId, student.Id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> KiemTraGiaoDich(string transactionCode)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return Unauthorized();

        var result = await _paymentService.CheckPaymentStatusAsync(transactionCode, student.Id);
        return Ok(result);
    }
}
