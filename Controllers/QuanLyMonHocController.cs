using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyMonHocController : Controller
{
    private const int PageSize = 10;
    private readonly ApplicationDbContext _db;

    public QuanLyMonHocController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
    {
        page = Math.Max(1, page);
        var query = _db.MonHocs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(item => item.MaMon.Contains(keyword) || item.TenMon.Contains(keyword));
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Min(page, totalPages);
        var model = new PagedResult<MonHoc>
        {
            Items = await query.OrderBy(item => item.MaMon)
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

    [HttpGet]
    public IActionResult Create() => View("Form", new MonHocFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MonHocFormViewModel model)
    {
        Normalize(model);
        await ValidateCodeAsync(model.MaMon);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _db.MonHocs.Add(new MonHoc
        {
            MaMon = model.MaMon,
            TenMon = model.TenMon,
            SoTinChi = model.SoTinChi
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaMon), "Mã môn học đã tồn tại.");
            return View("Form", model);
        }

        TempData["Msg"] = "Đã thêm môn học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var monHoc = await _db.MonHocs.FindAsync(id);
        if (monHoc == null)
        {
            return NotFound();
        }

        return View("Form", new MonHocFormViewModel
        {
            Id = monHoc.Id,
            MaMon = monHoc.MaMon,
            TenMon = monHoc.TenMon,
            SoTinChi = monHoc.SoTinChi
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MonHocFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var monHoc = await _db.MonHocs.FindAsync(id);
        if (monHoc == null)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateCodeAsync(model.MaMon, id);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        monHoc.MaMon = model.MaMon;
        monHoc.TenMon = model.TenMon;
        monHoc.SoTinChi = model.SoTinChi;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaMon), "Mã môn học đã tồn tại.");
            return View("Form", model);
        }

        TempData["Msg"] = "Đã cập nhật môn học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var monHoc = await _db.MonHocs.FindAsync(id);
        if (monHoc == null)
        {
            return NotFound();
        }

        if (await _db.Diems.AnyAsync(diem => diem.MonHocId == id))
        {
            TempData["Error"] = $"Không thể xóa môn học {monHoc.MaMon} vì đã có điểm được ghi nhận.";
            return RedirectToAction(nameof(Index));
        }

        _db.MonHocs.Remove(monHoc);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            TempData["Error"] = $"Không thể xóa môn học {monHoc.MaMon} vì đã có điểm được ghi nhận.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Msg"] = "Đã xóa môn học.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCodeAsync(string code, int? exceptId = null)
    {
        if (await _db.MonHocs.AnyAsync(item =>
                item.MaMon == code && (!exceptId.HasValue || item.Id != exceptId.Value)))
        {
            ModelState.AddModelError(nameof(MonHocFormViewModel.MaMon), "Mã môn học đã tồn tại.");
        }
    }

    private static void Normalize(MonHocFormViewModel model)
    {
        model.MaMon = model.MaMon?.Trim().ToUpperInvariant() ?? "";
        model.TenMon = model.TenMon?.Trim() ?? "";
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static bool IsForeignKeyViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 547 };
}
