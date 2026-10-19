using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLySinhVienController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuanLySinhVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? lopId, string? trangThai, int page = 1)
    {
        const int pageSize = 10;
        page = Math.Max(page, 1);

        var query = _db.SinhViens
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
                .ThenInclude(d => d.MonHoc)
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

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(s => s.TrangThai == trangThai);
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
        ViewBag.TrangThai = trangThai;
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
            .Include(s => s.Diems)
                .ThenInclude(d => d.MonHoc)
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
        return View("Form", new SinhVienFormViewModel { TrangThai = "Đang học" });
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
            LopId = model.LopId,
            NienKhoa = model.NienKhoa,
            TrangThai = model.TrangThai
        };

        _db.SinhViens.Add(sinhVien);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm sinh viên";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sinhVien = await _db.SinhViens
            .Include(s => s.Diems)
                .ThenInclude(d => d.MonHoc)
            .FirstOrDefaultAsync(s => s.Id == id);

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
            LopId = sinhVien.LopId,
            NienKhoa = sinhVien.NienKhoa,
            TrangThai = sinhVien.TrangThai,
            Gpa = sinhVien.Gpa,
            XepLoai = sinhVien.XepLoai
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
        sinhVien.NienKhoa = model.NienKhoa;
        sinhVien.TrangThai = model.TrangThai;

        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã cập nhật sinh viên";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien == null)
        {
            return NotFound();
        }

        if (sinhVien.UserId == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Không thể xóa hồ sơ đang gắn với tài khoản Admin hiện tại.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.SinhViens.Remove(sinhVien);
        await _db.SaveChangesAsync();

        if (sinhVien.UserId != null)
        {
            var user = await _userManager.FindByIdAsync(sinhVien.UserId);
            if (user != null)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Không thể xóa tài khoản sinh viên: {string.Join("; ", deleteResult.Errors.Select(error => error.Description))}";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        await transaction.CommitAsync();
        TempData["Msg"] = "Đã xóa sinh viên, điểm học phần và tài khoản liên kết (nếu có).";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLopAsync(int? selected = null)
    {
        var lops = await _db.Lops
            .Include(lop => lop.Nganh)
            .ThenInclude(nganh => nganh!.DonVi)
            .OrderBy(lop => lop.TenLop)
            .ToListAsync();

        ViewBag.DanhSachLop = new SelectList(lops, "Id", "TenLop", selected);
        ViewBag.KhoaTheoLop = lops.ToDictionary(
            lop => lop.Id.ToString(),
            lop => lop.Nganh?.DonVi?.TenDonVi ?? lop.Nganh?.TenKhoa ?? "");
        ViewBag.KhoaHienTai = selected.HasValue
            ? lops.FirstOrDefault(lop => lop.Id == selected.Value)?.Nganh?.DonVi?.TenDonVi
                ?? lops.FirstOrDefault(lop => lop.Id == selected.Value)?.Nganh?.TenKhoa
                ?? ""
            : "";
    }
}
