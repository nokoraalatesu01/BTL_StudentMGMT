using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyGiangVienController : Controller
{
    private const int PageSize = 10;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuanLyGiangVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
    {
        page = Math.Max(1, page);
        var query = _db.GiangViens.AsNoTracking().Include(item => item.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(item => item.MaGiangVien.Contains(keyword) || item.HoTen.Contains(keyword));
        }

        var totalItems = await query.CountAsync();
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize)));
        var model = new PagedResult<GiangVien>
        {
            Items = await query.OrderBy(item => item.MaGiangVien)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(),
            Page = page,
            PageSize = PageSize,
            TotalItems = totalItems
        };

        ViewBag.TuKhoa = tuKhoa;
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var giangVien = await _db.GiangViens
            .AsNoTracking()
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Id == id);
        return giangVien == null ? NotFound() : View(giangVien);
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new GiangVienFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GiangVienFormViewModel model)
    {
        Normalize(model);
        await ValidateCodeAsync(model.MaGiangVien);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _db.GiangViens.Add(new GiangVien
        {
            MaGiangVien = model.MaGiangVien,
            HoTen = model.HoTen,
            Email = model.Email,
            SoDienThoai = model.SoDienThoai
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaGiangVien), "Mã giảng viên đã tồn tại.");
            return View("Form", model);
        }

        TempData["Msg"] = "Đã thêm giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var giangVien = await _db.GiangViens.FindAsync(id);
        if (giangVien == null)
        {
            return NotFound();
        }

        return View("Form", new GiangVienFormViewModel
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
    public async Task<IActionResult> Edit(int id, GiangVienFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var giangVien = await _db.GiangViens.FindAsync(id);
        if (giangVien == null)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateCodeAsync(model.MaGiangVien, id);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        giangVien.MaGiangVien = model.MaGiangVien;
        giangVien.HoTen = model.HoTen;
        giangVien.Email = model.Email;
        giangVien.SoDienThoai = model.SoDienThoai;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaGiangVien), "Mã giảng viên đã tồn tại.");
            return View("Form", model);
        }

        TempData["Msg"] = "Đã cập nhật giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var giangVien = await _db.GiangViens.FirstOrDefaultAsync(item => item.Id == id);
        if (giangVien == null)
        {
            return NotFound();
        }

        if (giangVien.UserId == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Không thể xóa hồ sơ đang gắn với tài khoản Admin hiện tại.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.GiangViens.Remove(giangVien);
        await _db.SaveChangesAsync();

        if (giangVien.UserId != null)
        {
            var user = await _userManager.FindByIdAsync(giangVien.UserId);
            if (user != null)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Không thể xóa tài khoản giảng viên: {string.Join("; ", deleteResult.Errors.Select(error => error.Description))}";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        await transaction.CommitAsync();
        TempData["Msg"] = "Đã xóa hồ sơ giảng viên và tài khoản liên kết (nếu có).";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCodeAsync(string code, int? exceptId = null)
    {
        if (await _db.GiangViens.AnyAsync(item =>
                item.MaGiangVien == code && (!exceptId.HasValue || item.Id != exceptId.Value)))
        {
            ModelState.AddModelError(nameof(GiangVienFormViewModel.MaGiangVien), "Mã giảng viên đã tồn tại.");
        }
    }

    private static void Normalize(GiangVienFormViewModel model)
    {
        model.MaGiangVien = model.MaGiangVien.Trim();
        model.HoTen = model.HoTen.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
