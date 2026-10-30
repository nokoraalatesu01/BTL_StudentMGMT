using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        var query = _db.MonHocs
            .AsNoTracking()
            .Include(item => item.DonVi)
            .AsQueryable();

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
    public async Task<IActionResult> Create()
    {
        await LoadDonViDropdownAsync();
        return View("Form", new MonHocFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MonHocFormViewModel model)
    {
        Normalize(model);
        await ValidateCodeAsync(model.MaMon);
        if (!ModelState.IsValid)
        {
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        _db.MonHocs.Add(new MonHoc
        {
            MaMon = model.MaMon,
            TenMon = model.TenMon,
            SoTinChi = model.SoTinChi,
            DonViId = model.DonViId
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaMon), "Mã môn học đã tồn tại.");
            await LoadDonViDropdownAsync(model.DonViId);
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

        await LoadDonViDropdownAsync(monHoc.DonViId);
        return View("Form", new MonHocFormViewModel
        {
            Id = monHoc.Id,
            MaMon = monHoc.MaMon,
            TenMon = monHoc.TenMon,
            SoTinChi = monHoc.SoTinChi,
            DonViId = monHoc.DonViId
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
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        monHoc.MaMon = model.MaMon;
        monHoc.TenMon = model.TenMon;
        monHoc.SoTinChi = model.SoTinChi;
        monHoc.DonViId = model.DonViId;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaMon), "Mã môn học đã tồn tại.");
            await LoadDonViDropdownAsync(model.DonViId);
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

        if (await _db.Diems.AnyAsync(item => item.MonHocId == id))
        {
            TempData["Error"] = "Không thể xóa môn học đã có điểm sinh viên.";
            return RedirectToAction(nameof(Index));
        }

        _db.MonHocs.Remove(monHoc);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa môn học.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(MonHocFormViewModel model)
    {
        model.MaMon = (model.MaMon ?? "").Trim().ToUpperInvariant();
        model.TenMon = (model.TenMon ?? "").Trim();
    }

    private async Task ValidateCodeAsync(string maMon, int? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(maMon))
        {
            return;
        }

        var query = _db.MonHocs.AsNoTracking().Where(item => item.MaMon == maMon);
        if (currentId.HasValue)
        {
            query = query.Where(item => item.Id != currentId.Value);
        }

        if (await query.AnyAsync())
        {
            ModelState.AddModelError(nameof(MonHocFormViewModel.MaMon), "Mã môn học đã tồn tại.");
        }
    }

    private async Task LoadDonViDropdownAsync(int? selectedId = null)
    {
        var units = await _db.DonVis.AsNoTracking().Where(d => d.TrangThai == "ACTIVE").OrderBy(d => d.TenDonVi).ToListAsync();
        ViewBag.DonViList = new SelectList(units, nameof(DonVi.DonViId), nameof(DonVi.TenDonVi), selectedId);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
