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

        return View(giangVien);
    }

    public async Task<IActionResult> DanhSachSinhVien(string? tuKhoa, int? lopId, int page = 1)
    {
        const int pageSize = 10;

        var query = _db.SinhViens
            .Include(s => s.Lop)
            .ThenInclude(l => l!.Nganh)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(s => s.MaSinhVien.Contains(keyword) || s.HoTen.Contains(keyword));
        }

        if (lopId.HasValue)
        {
            query = query.Where(s => s.LopId == lopId.Value);
        }

        var totalItems = await query.CountAsync();
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

        return View(new PagedResult<SinhVien>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<IActionResult> ChiTietSinhVien(int id)
    {
        var sinhVien = await _db.SinhViens
            .Include(s => s.Lop)
            .ThenInclude(l => l!.Nganh)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sinhVien == null)
        {
            return NotFound();
        }

        return View(sinhVien);
    }

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

        return View(new CapNhatGiangVienViewModel
        {
            Id = giangVien.Id,
            MaGiangVien = giangVien.MaGiangVien,
            HoTen = giangVien.HoTen,
            Email = giangVien.Email,
            SoDienThoai = giangVien.SoDienThoai
        });
    }

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

        giangVien.HoTen = model.HoTen.Trim();
        giangVien.Email = model.Email?.Trim();
        giangVien.SoDienThoai = model.SoDienThoai?.Trim();

        user.HoTen = giangVien.HoTen;
        user.Email = giangVien.Email;
        user.PhoneNumber = giangVien.SoDienThoai;
        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
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
}
