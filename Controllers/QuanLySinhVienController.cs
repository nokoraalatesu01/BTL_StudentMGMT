using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLySinhVienController : Controller
{
    private readonly ApplicationDbContext _db;

    public QuanLySinhVienController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? lopId, int page = 1)
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

    public async Task<IActionResult> Details(int id)
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
    public async Task<IActionResult> Create()
    {
        await LoadLopAsync();
        return View("Form", new SinhVienFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SinhVienFormViewModel model)
    {
        if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == model.MaSinhVien))
        {
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadLopAsync(model.LopId);
            return View("Form", model);
        }

        var sinhVien = new SinhVien
        {
            MaSinhVien = model.MaSinhVien,
            HoTen = model.HoTen,
            NgaySinh = model.NgaySinh,
            GioiTinh = model.GioiTinh,
            Email = model.Email,
            SoDienThoai = model.SoDienThoai,
            DiaChi = model.DiaChi,
            LopId = model.LopId
        };

        _db.SinhViens.Add(sinhVien);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm sinh viên";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien == null)
        {
            return NotFound();
        }

        await LoadLopAsync(sinhVien.LopId);

        return View("Form", new SinhVienFormViewModel
        {
            Id = sinhVien.Id,
            MaSinhVien = sinhVien.MaSinhVien,
            HoTen = sinhVien.HoTen,
            NgaySinh = sinhVien.NgaySinh,
            GioiTinh = sinhVien.GioiTinh,
            Email = sinhVien.Email,
            SoDienThoai = sinhVien.SoDienThoai,
            DiaChi = sinhVien.DiaChi,
            LopId = sinhVien.LopId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SinhVienFormViewModel model)
    {
        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien == null)
        {
            return NotFound();
        }

        if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == model.MaSinhVien && s.Id != id))
        {
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadLopAsync(model.LopId);
            return View("Form", model);
        }

        sinhVien.MaSinhVien = model.MaSinhVien;
        sinhVien.HoTen = model.HoTen;
        sinhVien.NgaySinh = model.NgaySinh;
        sinhVien.GioiTinh = model.GioiTinh;
        sinhVien.Email = model.Email;
        sinhVien.SoDienThoai = model.SoDienThoai;
        sinhVien.DiaChi = model.DiaChi;
        sinhVien.LopId = model.LopId;

        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã cập nhật sinh viên";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien != null)
        {
            _db.SinhViens.Remove(sinhVien);
            await _db.SaveChangesAsync();
            TempData["Msg"] = "Đã xóa sinh viên";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLopAsync(int? selected = null)
    {
        ViewBag.DanhSachLop = new SelectList(
            await _db.Lops.OrderBy(l => l.TenLop).ToListAsync(),
            "Id",
            "TenLop",
            selected);
    }
}
