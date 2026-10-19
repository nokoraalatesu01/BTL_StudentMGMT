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

    // 4. Cập nhật thông tin giảng viên (GET)
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

    // 5. Cập nhật thông tin giảng viên (POST)
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
