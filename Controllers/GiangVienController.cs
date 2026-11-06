using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "GiangVien")]
public class GiangVienController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly ITimetableService _timetableService;

    public GiangVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService, ITimetableService timetableService)
    {
        _db = db;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _timetableService = timetableService;
    }

    // 1. Trang tổng quan giảng viên
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        GiangVien? giangVien = null;

        if (user != null)
        {
            giangVien = await _db.GiangViens
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.UserId == user.Id);
        }

        ViewBag.TongSoSinhVien = await _db.SinhViens.CountAsync();
        ViewBag.TongSoLop = await _db.Lops.CountAsync();
        ViewBag.TongSoNganh = await _db.Nganhs.CountAsync();
        ViewBag.TongSoBuoiDay = giangVien != null
            ? (await _db.LopHocPhans.CountAsync(l => l.GiangVienId == giangVien.Id) is int cnt && cnt > 0 ? cnt : await _db.LichGiangDays.CountAsync(l => l.GiangVienId == giangVien.Id))
            : await _db.LopHocPhans.CountAsync();

        return View(giangVien);
    }

    // 2. Danh sách sinh viên (tìm kiếm, lọc theo lớp, phân trang)
    public async Task<IActionResult> DanhSachSinhVien(string? tuKhoa, int? lopId, int page = 1)
    {
        int pageSize = 10;
        page = Math.Max(page, 1);

        var query = _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop)
            .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
            .ThenInclude(d => d.MonHoc)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(s => s.MaSinhVien.Contains(keyword) || s.HoTen.Contains(keyword));
        }

        if (lopId.HasValue && lopId > 0)
        {
            query = query.Where(s => s.LopId == lopId.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page > Math.Max(totalPages, 1))
        {
            page = Math.Max(totalPages, 1);
        }

        var items = await query
            .OrderBy(s => s.MaSinhVien)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LopId = lopId;
        ViewBag.DanhSachLop = new SelectList(
            await _db.Lops.OrderBy(l => l.TenLop).ToListAsync(),
            "Id",
            "TenLop",
            lopId);

        var model = new PagedResult<SinhVien>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View(model);
    }

    // 3. Chi tiết sinh viên
    public async Task<IActionResult> ChiTietSinhVien(int id)
    {
        var sinhVien = await _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop)
            .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
            .ThenInclude(d => d.MonHoc)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sinhVien == null)
        {
            return NotFound();
        }

        return View(sinhVien);
    }

    // 4. Lịch giảng dạy & Thời khóa biểu của giảng viên (Chức năng 4)
    public async Task<IActionResult> LichGiangDay(int? hocKyId, int? namHocId, int? monHocId, string? ngay, string? viewMode)
    {
        var user = await _userManager.GetUserAsync(User);
        GiangVien? giangVien = null;
        if (user != null)
        {
            giangVien = await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
        }

        if (giangVien == null)
        {
            TempData["Error"] = "Tài khoản của bạn chưa được liên kết với hồ sơ Giảng viên.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await _timetableService.GetGiangVienLichGiangDayAsync(
            giangVien.Id,
            hocKyId,
            namHocId,
            monHocId,
            ngay,
            viewMode ?? "week");

        return View(viewModel);
    }

    // 4.1. API Chi tiết buổi học (phục vụ Modal popup)
    [HttpGet]
    public async Task<IActionResult> ChiTietBuoiHoc(int lopHocPhanId, DateTime? ngay)
    {
        var user = await _userManager.GetUserAsync(User);
        GiangVien? giangVien = null;
        if (user != null)
        {
            giangVien = await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
        }

        var detail = await _timetableService.GetChiTietBuoiHocAsync(
            lopHocPhanId,
            ngay,
            currentGiangVienId: giangVien?.Id);

        if (detail == null) return NotFound(new { success = false, message = "Không tìm thấy buổi học" });
        return Json(new { success = true, data = detail });
    }

    // 5. Điểm danh sinh viên theo buổi học / Lớp học phần
    public async Task<IActionResult> DiemDanh(int? lopHocPhanId, int? lopId, int? monHocId, DateTime? ngay)
    {
        var targetDate = (ngay ?? DateTime.Today).Date;
        ViewBag.NgayChon = targetDate.ToString("yyyy-MM-dd");

        var user = await _userManager.GetUserAsync(User);
        GiangVien? giangVien = null;
        if (user != null)
        {
            giangVien = await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
        }

        // Ưu tiên điểm danh theo chuẩn Lớp học phần tín chỉ
        var dsLhp = await _db.LopHocPhans
            .AsNoTracking()
            .Include(l => l.MonHoc)
            .Where(l => giangVien == null || l.GiangVienId == giangVien.Id)
            .OrderBy(l => l.TenLopHocPhan)
            .ToListAsync();

        var selectedLhpId = (lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
            ? lopHocPhanId.Value
            : (dsLhp.FirstOrDefault()?.Id ?? 0);

        ViewBag.DanhSachLopHocPhan = new SelectList(
            dsLhp.Select(l => new { l.Id, Display = $"{l.MaLopHocPhan} - {l.TenLopHocPhan}" }),
            "Id",
            "Display",
            selectedLhpId);

        var model = new DiemDanhFormViewModel
        {
            LopHocPhanId = selectedLhpId,
            NgayDiemDanh = targetDate,
            DanhSachSinhVien = new List<DiemDanhItemViewModel>()
        };

        if (selectedLhpId > 0)
        {
            var lhp = await _db.LopHocPhans
                .AsNoTracking()
                .Include(l => l.MonHoc)
                .Include(l => l.DangKyHocPhans).ThenInclude(dk => dk.SinhVien).ThenInclude(sv => sv.Lop)
                .FirstOrDefaultAsync(l => l.Id == selectedLhpId);

            if (lhp != null)
            {
                ViewBag.SelectedLhp = lhp;
                model.TenLopHocPhan = lhp.TenLopHocPhan;
                model.MonHocId = lhp.MonHocId;

                var enrolledStudents = lhp.DangKyHocPhans
                    .Where(dk => dk.TrangThai == "ENROLLED")
                    .Select(dk => dk.SinhVien)
                    .OrderBy(sv => sv.MaSinhVien)
                    .ToList();

                var dsDiemDanhCu = await _db.DiemDanhs
                    .AsNoTracking()
                    .Where(d => d.LopHocPhanId == selectedLhpId && d.NgayDiemDanh.Date == targetDate)
                    .ToListAsync();

                foreach (var sv in enrolledStudents)
                {
                    var daDiemDanh = dsDiemDanhCu.FirstOrDefault(d => d.SinhVienId == sv.Id);
                    model.DanhSachSinhVien.Add(new DiemDanhItemViewModel
                    {
                        SinhVienId = sv.Id,
                        MaSinhVien = sv.MaSinhVien,
                        HoTen = sv.HoTen,
                        TenLopHanhChinh = sv.Lop?.TenLop ?? "Tự do",
                        TrangThai = daDiemDanh?.TrangThai ?? "Có mặt",
                        GhiChu = daDiemDanh?.GhiChu
                    });
                }

                return View(model);
            }
        }

        // Fallback: Nếu không có lớp học phần, hỗ trợ lớp hành chính cũ
        var dsLop = await _db.Lops.OrderBy(l => l.TenLop).ToListAsync();
        var selectedLopId = (lopId.HasValue && lopId.Value > 0) ? lopId.Value : (dsLop.FirstOrDefault()?.Id ?? 0);
        var dsMon = await _db.MonHocs.OrderBy(m => m.TenMon).ToListAsync();
        var selectedMonId = (monHocId.HasValue && monHocId.Value > 0) ? monHocId.Value : (dsMon.FirstOrDefault()?.Id ?? 0);

        ViewBag.DanhSachLop = new SelectList(dsLop, "Id", "TenLop", selectedLopId);
        ViewBag.DanhSachMonHoc = new SelectList(dsMon, "Id", "TenMon", selectedMonId);
        model.LopId = selectedLopId;
        model.MonHocId = selectedMonId;

        if (selectedLopId > 0 && selectedMonId > 0)
        {
            var sinhViens = await _db.SinhViens
                .Where(s => s.LopId == selectedLopId)
                .OrderBy(s => s.MaSinhVien)
                .ToListAsync();

            var dsDiemDanhCu = await _db.DiemDanhs
                .Where(d => d.LopId == selectedLopId && d.MonHocId == selectedMonId && d.NgayDiemDanh.Date == targetDate)
                .ToListAsync();

            foreach (var sv in sinhViens)
            {
                var daDiemDanh = dsDiemDanhCu.FirstOrDefault(d => d.SinhVienId == sv.Id);
                model.DanhSachSinhVien.Add(new DiemDanhItemViewModel
                {
                    SinhVienId = sv.Id,
                    MaSinhVien = sv.MaSinhVien,
                    HoTen = sv.HoTen,
                    TenLopHanhChinh = sv.Lop?.TenLop,
                    TrangThai = daDiemDanh?.TrangThai ?? "Có mặt",
                    GhiChu = daDiemDanh?.GhiChu
                });
            }
        }

        return View(model);
    }

    // 6. Lưu điểm danh (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LuuDiemDanh(DiemDanhFormViewModel model)
    {
        var targetDate = model.NgayDiemDanh.Date;

        if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0)
        {
            var lhp = await _db.LopHocPhans.FirstOrDefaultAsync(l => l.Id == model.LopHocPhanId.Value);
            var monHocId = lhp?.MonHocId ?? model.MonHocId;

            if (model.DanhSachSinhVien != null && model.DanhSachSinhVien.Count > 0)
            {
                foreach (var item in model.DanhSachSinhVien)
                {
                    var record = await _db.DiemDanhs.FirstOrDefaultAsync(d =>
                        d.SinhVienId == item.SinhVienId &&
                        d.LopHocPhanId == model.LopHocPhanId.Value &&
                        d.NgayDiemDanh.Date == targetDate);

                    if (record != null)
                    {
                        record.TrangThai = item.TrangThai;
                        record.GhiChu = item.GhiChu;
                    }
                    else
                    {
                        _db.DiemDanhs.Add(new DiemDanh
                        {
                            SinhVienId = item.SinhVienId,
                            LopHocPhanId = model.LopHocPhanId.Value,
                            MonHocId = monHocId,
                            NgayDiemDanh = targetDate,
                            TrangThai = item.TrangThai,
                            GhiChu = item.GhiChu
                        });
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Msg"] = $"Đã lưu kết quả điểm danh ngày {targetDate:dd/MM/yyyy} thành công!";
            }

            return RedirectToAction(nameof(DiemDanh), new
            {
                lopHocPhanId = model.LopHocPhanId.Value,
                ngay = targetDate.ToString("yyyy-MM-dd")
            });
        }

        // Fallback: Điểm danh theo lớp hành chính
        if (model.LopId <= 0 || model.MonHocId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn lớp học và môn học hợp lệ trước khi điểm danh.";
            return RedirectToAction(nameof(DiemDanh));
        }

        if (model.DanhSachSinhVien != null && model.DanhSachSinhVien.Count > 0)
        {
            foreach (var item in model.DanhSachSinhVien)
            {
                var record = await _db.DiemDanhs.FirstOrDefaultAsync(d =>
                    d.SinhVienId == item.SinhVienId &&
                    d.LopId == model.LopId &&
                    d.MonHocId == model.MonHocId &&
                    d.NgayDiemDanh.Date == targetDate);

                if (record != null)
                {
                    record.TrangThai = item.TrangThai;
                    record.GhiChu = item.GhiChu;
                }
                else
                {
                    _db.DiemDanhs.Add(new DiemDanh
                    {
                        SinhVienId = item.SinhVienId,
                        LopId = model.LopId,
                        MonHocId = model.MonHocId,
                        NgayDiemDanh = targetDate,
                        TrangThai = item.TrangThai,
                        GhiChu = item.GhiChu
                    });
                }
            }

            await _db.SaveChangesAsync();
            TempData["Msg"] = $"Đã lưu kết quả điểm danh ngày {targetDate:dd/MM/yyyy} thành công!";
        }
        else
        {
            TempData["Msg"] = "Lớp hiện chưa có sinh viên nào để lưu điểm danh.";
        }

        return RedirectToAction(nameof(DiemDanh), new
        {
            lopId = model.LopId,
            monHocId = model.MonHocId,
            ngay = targetDate.ToString("yyyy-MM-dd")
        });
    }

    // 7. Cập nhật thông tin giảng viên (GET)
    [HttpGet]
    public async Task<IActionResult> CapNhatThongTin()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var giangVien = await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
        if (giangVien == null)
        {
            return NotFound("Không tìm thấy thông tin giảng viên");
        }

        var model = new CapNhatGiangVienViewModel
        {
            Id = giangVien.Id,
            MaGiangVien = giangVien.MaGiangVien,
            HoTen = giangVien.HoTen,
            Email = giangVien.Email,
            SoDienThoai = giangVien.SoDienThoai,
            HinhAnh = giangVien.HinhAnh
        };

        return View(model);
    }

    // 8. Cập nhật thông tin giảng viên (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatThongTin(CapNhatGiangVienViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var giangVien = await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
        if (giangVien == null)
        {
            return NotFound("Không tìm thấy thông tin giảng viên");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.FileAnh != null && model.FileAnh.Length > 0)
        {
            var avatarPath = await _fileUploadService.UploadAvatarAsync(model.FileAnh, giangVien.HinhAnh);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                giangVien.HinhAnh = avatarPath;
            }
        }

        giangVien.HoTen = model.HoTen.Trim();
        giangVien.Email = model.Email?.Trim();
        giangVien.SoDienThoai = model.SoDienThoai?.Trim();

        user.HoTen = giangVien.HoTen;
        user.Email = giangVien.Email;
        user.PhoneNumber = giangVien.SoDienThoai;
        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrEmpty(model.MatKhauMoi))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await _userManager.ResetPasswordAsync(user, resetToken, model.MatKhauMoi);
            if (!passResult.Succeeded)
            {
                foreach (var err in passResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }
        }

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Cập nhật thông tin cá nhân thành công!";
        return RedirectToAction(nameof(Index));
    }

    // 10. Đổi ảnh đại diện nhanh cho giảng viên
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatAnhDaiDien(IFormFile? fileAnh, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Index));

        var giangVien = await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
        if (giangVien == null) return RedirectToAction(nameof(Index));

        if (fileAnh != null && fileAnh.Length > 0)
        {
            var avatarPath = await _fileUploadService.UploadAvatarAsync(fileAnh, giangVien.HinhAnh);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                giangVien.HinhAnh = avatarPath;
                await _db.SaveChangesAsync();
                TempData["Msg"] = "Cập nhật ảnh hồ sơ giảng viên thành công!";
            }
            else
            {
                TempData["Error"] = "Định dạng file không hợp lệ (.jpg, .jpeg, .png, .webp) hoặc vượt quá 5MB.";
            }
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}

