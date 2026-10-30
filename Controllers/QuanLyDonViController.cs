using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyDonViController : Controller
{
    private const int PageSize = 10;
    private readonly ApplicationDbContext _db;

    public QuanLyDonViController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? tuKhoa, string? loaiDonVi, string? trangThai, int page = 1)
    {
        page = Math.Max(1, page);
        var query = _db.DonVis
            .AsNoTracking()
            .Include(d => d.DonViCha)
            .Include(d => d.GiangViens)
            .Include(d => d.MonHocs)
            .Include(d => d.Nganhs)
            .Include(d => d.InverseDonViCha)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(d => d.MaDonVi.Contains(keyword) || d.TenDonVi.Contains(keyword) || (d.Email != null && d.Email.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(loaiDonVi))
        {
            query = query.Where(d => d.LoaiDonVi == loaiDonVi);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(d => d.TrangThai == trangThai);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Min(page, totalPages);

        var items = await query
            .OrderBy(d => d.LoaiDonVi)
            .ThenBy(d => d.MaDonVi)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(d => new DonViListItemViewModel
            {
                DonViId = d.DonViId,
                MaDonVi = d.MaDonVi,
                TenDonVi = d.TenDonVi,
                LoaiDonVi = d.LoaiDonVi,
                TenDonViCha = d.DonViCha != null ? d.DonViCha.TenDonVi : null,
                Email = d.Email,
                DienThoai = d.DienThoai,
                TrangThai = d.TrangThai,
                SoGiangVien = d.GiangViens.Count,
                SoMonHoc = d.MonHocs.Count,
                SoNganh = d.Nganhs.Count,
                SoDonViCon = d.InverseDonViCha.Count
            })
            .ToListAsync();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LoaiDonVi = loaiDonVi;
        ViewBag.TrangThai = trangThai;

        return View(new PagedResult<DonViListItemViewModel>
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadParentUnitsDropdownAsync();
        return View("Form", new DonViFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonViFormViewModel model)
    {
        Normalize(model);

        if (await _db.DonVis.AnyAsync(d => d.MaDonVi == model.MaDonVi))
        {
            ModelState.AddModelError(nameof(model.MaDonVi), "Mã đơn vị đã tồn tại trong hệ thống.");
        }

        if (!ModelState.IsValid)
        {
            await LoadParentUnitsDropdownAsync();
            return View("Form", model);
        }

        var donVi = new DonVi
        {
            MaDonVi = model.MaDonVi,
            TenDonVi = model.TenDonVi,
            LoaiDonVi = model.LoaiDonVi,
            DonViChaId = model.DonViChaId,
            Email = model.Email,
            DienThoai = model.DienThoai,
            TrangThai = model.TrangThai,
            CreatedAt = DateTime.UtcNow
        };

        _db.DonVis.Add(donVi);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm mới đơn vị thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var donVi = await _db.DonVis.FindAsync(id);
        if (donVi == null)
        {
            return NotFound();
        }

        await LoadParentUnitsDropdownAsync(id, donVi.DonViChaId);
        return View("Form", new DonViFormViewModel
        {
            DonViId = donVi.DonViId,
            MaDonVi = donVi.MaDonVi,
            TenDonVi = donVi.TenDonVi,
            LoaiDonVi = donVi.LoaiDonVi,
            DonViChaId = donVi.DonViChaId,
            Email = donVi.Email,
            DienThoai = donVi.DienThoai,
            TrangThai = donVi.TrangThai
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DonViFormViewModel model)
    {
        if (id != model.DonViId)
        {
            return BadRequest();
        }

        var donVi = await _db.DonVis.FindAsync(id);
        if (donVi == null)
        {
            return NotFound();
        }

        Normalize(model);

        if (await _db.DonVis.AnyAsync(d => d.MaDonVi == model.MaDonVi && d.DonViId != id))
        {
            ModelState.AddModelError(nameof(model.MaDonVi), "Mã đơn vị đã tồn tại trong hệ thống.");
        }

        // Chống vòng lặp phân cấp cha-con (Circular dependency)
        if (model.DonViChaId == id)
        {
            ModelState.AddModelError(nameof(model.DonViChaId), "Đơn vị không thể chọn chính nó làm đơn vị cha.");
        }

        if (!ModelState.IsValid)
        {
            await LoadParentUnitsDropdownAsync(id, model.DonViChaId);
            return View("Form", model);
        }

        donVi.MaDonVi = model.MaDonVi;
        donVi.TenDonVi = model.TenDonVi;
        donVi.LoaiDonVi = model.LoaiDonVi;
        donVi.DonViChaId = model.DonViChaId;
        donVi.Email = model.Email;
        donVi.DienThoai = model.DienThoai;
        donVi.TrangThai = model.TrangThai;
        donVi.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã cập nhật thông tin đơn vị thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var donVi = await _db.DonVis
            .Include(d => d.GiangViens)
            .Include(d => d.MonHocs)
            .Include(d => d.Nganhs)
            .Include(d => d.InverseDonViCha)
            .FirstOrDefaultAsync(d => d.DonViId == id);

        if (donVi == null)
        {
            return NotFound();
        }

        // Kiểm tra toàn vẹn quan hệ trước khi cho phép xóa
        var conflicts = new List<string>();
        if (donVi.InverseDonViCha.Count > 0)
            conflicts.Add($"{donVi.InverseDonViCha.Count} đơn vị cấp dưới trực thuộc");
        if (donVi.GiangViens.Count > 0)
            conflicts.Add($"{donVi.GiangViens.Count} giảng viên");
        if (donVi.MonHocs.Count > 0)
            conflicts.Add($"{donVi.MonHocs.Count} môn học");
        if (donVi.Nganhs.Count > 0)
            conflicts.Add($"{donVi.Nganhs.Count} ngành đào tạo");

        if (conflicts.Count > 0)
        {
            // Tự động chuyển sang ngưng hoạt động (Soft Deactivate) thay vì để lỗi ràng buộc FK
            donVi.TrangThai = "INACTIVE";
            donVi.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Msg"] = $"Không thể xóa vật lý đơn vị '{donVi.TenDonVi}' do đang được liên kết với {string.Join(", ", conflicts)}. Đơn vị đã được tự động chuyển sang trạng thái 'NGỪNG HOẠT ĐỘNG' để bảo toàn dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        _db.DonVis.Remove(donVi);
        await _db.SaveChangesAsync();

        TempData["Msg"] = $"Đã xóa đơn vị '{donVi.TenDonVi}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(DonViFormViewModel model)
    {
        model.MaDonVi = (model.MaDonVi ?? "").Trim().ToUpperInvariant();
        model.TenDonVi = (model.TenDonVi ?? "").Trim();
        model.LoaiDonVi = (model.LoaiDonVi ?? "KHOA").Trim().ToUpperInvariant();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.DienThoai = string.IsNullOrWhiteSpace(model.DienThoai) ? null : model.DienThoai.Trim();
        model.TrangThai = string.IsNullOrWhiteSpace(model.TrangThai) ? "ACTIVE" : model.TrangThai.Trim().ToUpperInvariant();
    }

    private async Task LoadParentUnitsDropdownAsync(int? currentUnitId = null, int? selectedParentId = null)
    {
        var query = _db.DonVis.AsNoTracking().Where(d => d.TrangThai == "ACTIVE");
        if (currentUnitId.HasValue)
        {
            query = query.Where(d => d.DonViId != currentUnitId.Value);
        }

        var units = await query.OrderBy(d => d.TenDonVi).ToListAsync();
        ViewBag.DonViChaList = new SelectList(units, nameof(DonVi.DonViId), nameof(DonVi.TenDonVi), selectedParentId);
    }
}
