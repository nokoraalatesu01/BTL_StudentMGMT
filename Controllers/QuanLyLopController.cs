using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyLopController : Controller
{
    private readonly ApplicationDbContext _db;

    public QuanLyLopController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? nganhId, int page = 1)
    {
        const int pageSize = 10;
        page = Math.Max(page, 1);
        var query = _db.Lops.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(lop => lop.TenLop.Contains(keyword));
        }

        if (nganhId.HasValue)
        {
            query = query.Where(lop => lop.NganhId == nganhId.Value);
        }

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.NganhId = nganhId;
        ViewBag.DanhSachNganh = new SelectList(
            await _db.Nganhs.OrderBy(nganh => nganh.TenNganh).ToListAsync(),
            nameof(Nganh.Id),
            nameof(Nganh.TenNganh),
            nganhId);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page > Math.Max(totalPages, 1))
        {
            page = Math.Max(totalPages, 1);
        }

        var items = await query
            .OrderBy(lop => lop.Nganh!.TenNganh)
            .ThenBy(lop => lop.TenLop)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(lop => new LopListItemViewModel
            {
                Id = lop.Id,
                TenLop = lop.TenLop,
                NganhId = lop.NganhId,
                MaNganh = lop.Nganh!.MaNganh,
                TenNganh = lop.Nganh.TenNganh,
                SoSinhVien = lop.SinhViens.Count
            })
            .ToListAsync();

        return View(new PagedResult<LopListItemViewModel>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadNganhAsync();
        return View("Form", new LopFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LopFormViewModel model)
    {
        model.TenLop = model.TenLop.Trim();
        await ValidateNganhAndUniqueNameAsync(model);

        if (!ModelState.IsValid)
        {
            await LoadNganhAsync(model.NganhId);
            return View("Form", model);
        }

        _db.Lops.Add(new Lop
        {
            TenLop = model.TenLop,
            NganhId = model.NganhId
        });
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm lớp.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lop = await _db.Lops.FindAsync(id);
        if (lop == null)
        {
            return NotFound();
        }

        await LoadNganhAsync(lop.NganhId);
        return View("Form", new LopFormViewModel
        {
            Id = lop.Id,
            TenLop = lop.TenLop,
            NganhId = lop.NganhId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LopFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var lop = await _db.Lops.FindAsync(id);
        if (lop == null)
        {
            return NotFound();
        }

        model.TenLop = model.TenLop.Trim();
        await ValidateNganhAndUniqueNameAsync(model, id);

        if (!ModelState.IsValid)
        {
            await LoadNganhAsync(model.NganhId);
            return View("Form", model);
        }

        lop.TenLop = model.TenLop;
        lop.NganhId = model.NganhId;
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã cập nhật lớp.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lop = await _db.Lops
            .Include(item => item.SinhViens)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (lop == null)
        {
            return NotFound();
        }

        if (lop.SinhViens.Count > 0)
        {
            TempData["Error"] = "Không thể xóa lớp đang có sinh viên. Hãy chuyển sinh viên sang lớp khác trước.";
            return RedirectToAction(nameof(Index));
        }

        _db.Lops.Remove(lop);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa lớp.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateNganhAndUniqueNameAsync(LopFormViewModel model, int? exceptId = null)
    {
        if (!await _db.Nganhs.AnyAsync(nganh => nganh.Id == model.NganhId))
        {
            ModelState.AddModelError(nameof(model.NganhId), "Ngành được chọn không tồn tại.");
            return;
        }

        if (await _db.Lops.AnyAsync(lop =>
                lop.NganhId == model.NganhId &&
                lop.TenLop == model.TenLop &&
                (!exceptId.HasValue || lop.Id != exceptId.Value)))
        {
            ModelState.AddModelError(nameof(model.TenLop), "Tên lớp đã tồn tại trong ngành này.");
        }
    }

    private async Task LoadNganhAsync(int? selected = null)
    {
        ViewBag.DanhSachNganh = new SelectList(
            await _db.Nganhs.OrderBy(nganh => nganh.TenNganh).ToListAsync(),
            nameof(Nganh.Id),
            nameof(Nganh.TenNganh),
            selected);
    }
}
