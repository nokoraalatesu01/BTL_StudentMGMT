using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
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

    public GiangVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // 1. Trang tổng quan giảng viên
    public IActionResult Index()
    {
        var user = _userManager.GetUserAsync(User).Result;
        GiangVien? giangVien = null;

        if (user != null)
        {
            giangVien = _db.GiangViens
                .Include(g => g.User)
                .FirstOrDefault(g => g.UserId == user.Id);
        }

        ViewBag.TongSoSinhVien = _db.SinhViens.Count();
        ViewBag.TongSoLop = _db.Lops.Count();
        ViewBag.TongSoNganh = _db.Nganhs.Count();
        ViewBag.TongSoBuoiDay = _db.LichGiangDays.Count();

        return View(giangVien);
    }

    // 2. Danh sách sinh viên (tìm kiếm, lọc theo lớp, phân trang)
    public IActionResult DanhSachSinhVien(string? tuKhoa, int? lopId, int page = 1)
    {
        int pageSize = 10;
        page = Math.Max(page, 1);

        var query = _db.SinhViens
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

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page > Math.Max(totalPages, 1))
        {
            page = Math.Max(totalPages, 1);
        }

        var items = query
            .OrderBy(s => s.MaSinhVien)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LopId = lopId;
        ViewBag.DanhSachLop = new SelectList(
            _db.Lops.OrderBy(l => l.TenLop).ToList(),
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
    public IActionResult ChiTietSinhVien(int id)
    {
        var sinhVien = _db.SinhViens
            .Include(s => s.Lop)
            .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
            .ThenInclude(d => d.MonHoc)
            .FirstOrDefault(s => s.Id == id);

        if (sinhVien == null)
        {
            return NotFound();
        }

        return View(sinhVien);
    }

    // 4. Lịch giảng dạy & Thời khóa biểu của giảng viên (Chức năng 4)
    public IActionResult LichGiangDay()
    {
        var user = _userManager.GetUserAsync(User).Result;
        GiangVien? giangVien = null;
        if (user != null)
        {
            giangVien = _db.GiangViens.FirstOrDefault(g => g.UserId == user.Id);
        }

        var query = _db.LichGiangDays
            .Include(l => l.MonHoc)
            .Include(l => l.Lop)
            .AsQueryable();

        if (giangVien != null)
        {
            // Nếu có giảng viên thì lọc theo giảng viên, nếu chưa phân thì hiện lịch khoa
            var lichGv = query.Where(l => l.GiangVienId == giangVien.Id).ToList();
            if (lichGv.Any())
            {
                query = query.Where(l => l.GiangVienId == giangVien.Id);
            }
        }

        var list = query
            .OrderBy(l => l.Thu)
            .ThenBy(l => l.TietBatDau)
            .ToList();

        ViewBag.GiangVien = giangVien;
        return View(list);
    }

    // 5. Điểm danh sinh viên theo buổi học (Chức năng 2)
    public IActionResult DiemDanh(int? lopId, int? monHocId, DateTime? ngay)
    {
        var targetDate = (ngay ?? DateTime.Today).Date;

        // Chọn lớp mặc định nếu chưa chọn
        var dsLop = _db.Lops.OrderBy(l => l.TenLop).ToList();
        var selectedLopId = (lopId.HasValue && lopId.Value > 0) ? lopId.Value : (dsLop.FirstOrDefault()?.Id ?? 0);

        // Chọn môn học mặc định nếu chưa chọn
        var dsMon = _db.MonHocs.OrderBy(m => m.TenMon).ToList();
        var selectedMonId = (monHocId.HasValue && monHocId.Value > 0) ? monHocId.Value : (dsMon.FirstOrDefault()?.Id ?? 0);

        ViewBag.DanhSachLop = new SelectList(dsLop, "Id", "TenLop", selectedLopId);
        ViewBag.DanhSachMonHoc = new SelectList(dsMon, "Id", "TenMon", selectedMonId);
        ViewBag.NgayChon = targetDate.ToString("yyyy-MM-dd");

        // Lấy danh sách sinh viên của lớp
        var sinhViens = _db.SinhViens
            .Where(s => s.LopId == selectedLopId)
            .OrderBy(s => s.MaSinhVien)
            .ToList();

        // Lấy danh sách điểm danh đã có trong ngày đó và môn đó
        var dsDiemDanhCu = _db.DiemDanhs
            .Where(d => d.LopId == selectedLopId && d.MonHocId == selectedMonId && d.NgayDiemDanh.Date == targetDate)
            .ToList();

        var model = new DiemDanhFormViewModel
        {
            LopId = selectedLopId,
            MonHocId = selectedMonId,
            NgayDiemDanh = targetDate,
            DanhSachSinhVien = new List<DiemDanhItemViewModel>()
        };

        foreach (var sv in sinhViens)
        {
            var daDiemDanh = dsDiemDanhCu.FirstOrDefault(d => d.SinhVienId == sv.Id);
            model.DanhSachSinhVien.Add(new DiemDanhItemViewModel
            {
                SinhVienId = sv.Id,
                MaSinhVien = sv.MaSinhVien,
                HoTen = sv.HoTen,
                TrangThai = daDiemDanh?.TrangThai ?? "Có mặt",
                GhiChu = daDiemDanh?.GhiChu
            });
        }

        return View(model);
    }

    // 6. Lưu điểm danh (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LuuDiemDanh(DiemDanhFormViewModel model)
    {
        var targetDate = model.NgayDiemDanh.Date;

        foreach (var item in model.DanhSachSinhVien)
        {
            var record = _db.DiemDanhs.FirstOrDefault(d =>
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

        _db.SaveChanges();
        TempData["Msg"] = $"Đã lưu kết quả điểm danh ngày {targetDate:dd/MM/yyyy} thành công!";
        return RedirectToAction(nameof(DiemDanh), new
        {
            lopId = model.LopId,
            monHocId = model.MonHocId,
            ngay = targetDate.ToString("yyyy-MM-dd")
        });
    }

    // 7. Cập nhật thông tin giảng viên (GET)
    [HttpGet]
    public IActionResult CapNhatThongTin()
    {
        var user = _userManager.GetUserAsync(User).Result;
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var giangVien = _db.GiangViens.FirstOrDefault(g => g.UserId == user.Id);
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
            SoDienThoai = giangVien.SoDienThoai
        };

        return View(model);
    }

    // 8. Cập nhật thông tin giảng viên (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapNhatThongTin(CapNhatGiangVienViewModel model)
    {
        var user = _userManager.GetUserAsync(User).Result;
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var giangVien = _db.GiangViens.FirstOrDefault(g => g.UserId == user.Id);
        if (giangVien == null)
        {
            return NotFound("Không tìm thấy thông tin giảng viên");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        giangVien.HoTen = model.HoTen.Trim();
        giangVien.Email = model.Email?.Trim();
        giangVien.SoDienThoai = model.SoDienThoai?.Trim();

        user.HoTen = giangVien.HoTen;
        user.Email = giangVien.Email;
        user.PhoneNumber = giangVien.SoDienThoai;
        _userManager.UpdateAsync(user).Wait();

        if (!string.IsNullOrEmpty(model.MatKhauMoi))
        {
            var resetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;
            var passResult = _userManager.ResetPasswordAsync(user, resetToken, model.MatKhauMoi).Result;
            if (!passResult.Succeeded)
            {
                foreach (var err in passResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }
        }

        _db.SaveChanges();
        TempData["Msg"] = "Cập nhật thông tin cá nhân thành công!";
        return RedirectToAction(nameof(Index));
    }
}
