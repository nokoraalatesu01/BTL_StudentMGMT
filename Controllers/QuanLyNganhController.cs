using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyNganhController : Controller
{
    private readonly ApplicationDbContext _db;

    public QuanLyNganhController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? tuKhoa)
    {
        var query = _db.Nganhs
            .Include(nganh => nganh.DonVi)
            .Include(nganh => nganh.Lops)
            .Include(nganh => nganh.ChuongTrinhs)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(nganh =>
                nganh.MaNganh.Contains(keyword) ||
                nganh.TenNganh.Contains(keyword) ||
                nganh.TenKhoa.Contains(keyword));
        }

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.ChuongTrinhChuaGanNganh = await _db.ChuongTrinhDaoTaos
            .Where(program => program.NganhId == null)
            .OrderBy(program => program.LoaiChuongTrinh)
            .ThenBy(program => program.TenChuongTrinh)
            .ToListAsync();
        return View(await query
            .OrderBy(nganh => nganh.TenKhoa)
            .ThenBy(nganh => nganh.TenNganh)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var nganh = await _db.Nganhs
            .Include(item => item.Lops)
                .ThenInclude(lop => lop.SinhViens)
            .Include(item => item.ChuongTrinhs)
            .FirstOrDefaultAsync(item => item.Id == id);

        return nganh == null ? NotFound() : View(nganh);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDonViDropdownAsync();
        return View("Form", new NganhFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NganhFormViewModel model)
    {
        Normalize(model);
        await ValidateUniqueCodeAsync(model);
        await SynchronizeFacultyFromDonViAsync(model);
        ValidateFaculty(model);

        if (!ModelState.IsValid)
        {
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        _db.Nganhs.Add(new Nganh
        {
            MaNganh = model.MaNganh,
            TenNganh = model.TenNganh,
            TenKhoa = model.TenKhoa,
            DonViId = model.DonViId,
            TichHopCuNhanKySu = model.TichHopCuNhanKySu
        });
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm ngành.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var nganh = await _db.Nganhs.FindAsync(id);
        if (nganh == null)
        {
            return NotFound();
        }

        await LoadDonViDropdownAsync(nganh.DonViId);
        return View("Form", new NganhFormViewModel
        {
            Id = nganh.Id,
            MaNganh = nganh.MaNganh,
            TenNganh = nganh.TenNganh,
            TenKhoa = nganh.TenKhoa,
            DonViId = nganh.DonViId,
            TichHopCuNhanKySu = nganh.TichHopCuNhanKySu
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NganhFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var nganh = await _db.Nganhs.FindAsync(id);
        if (nganh == null)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateUniqueCodeAsync(model, id);
        await SynchronizeFacultyFromDonViAsync(model);
        ValidateFaculty(model);

        if (!ModelState.IsValid)
        {
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        nganh.MaNganh = model.MaNganh;
        nganh.TenNganh = model.TenNganh;
        nganh.TenKhoa = model.TenKhoa;
        nganh.DonViId = model.DonViId;
        nganh.TichHopCuNhanKySu = model.TichHopCuNhanKySu;
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã cập nhật ngành.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nganh = await _db.Nganhs
            .Include(item => item.Lops)
            .Include(item => item.ChuongTrinhs)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (nganh == null)
        {
            return NotFound();
        }

        if (nganh.Lops.Count > 0 || nganh.ChuongTrinhs.Count > 0)
        {
            TempData["Error"] = "Không thể xóa ngành đang có lớp hoặc chương trình đào tạo liên kết.";
            return RedirectToAction(nameof(Index));
        }

        _db.Nganhs.Remove(nganh);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa ngành.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportCatalog()
    {
        var existing = await _db.Nganhs.ToListAsync();
        var imported = 0;

        foreach (var program in NganhCatalog.Programs)
        {
            var current = existing.FirstOrDefault(nganh =>
                string.Equals(nganh.MaNganh, program.MaNganh, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(nganh.TenNganh.Trim(), program.TenNganh, StringComparison.OrdinalIgnoreCase));

            if (current != null)
            {
                current.MaNganh = program.MaNganh;
                current.TenNganh = program.TenNganh;
                current.TenKhoa = program.TenKhoa;
                current.TichHopCuNhanKySu = program.TichHopCuNhanKySu;
                continue;
            }

            var copy = new Nganh
            {
                MaNganh = program.MaNganh,
                TenNganh = program.TenNganh,
                TenKhoa = program.TenKhoa,
                TichHopCuNhanKySu = program.TichHopCuNhanKySu
            };
            _db.Nganhs.Add(copy);
            existing.Add(copy);
            imported++;
        }

        var majorsByCode = existing.ToDictionary(nganh => nganh.MaNganh, StringComparer.OrdinalIgnoreCase);
        var existingPrograms = await _db.ChuongTrinhDaoTaos.ToListAsync();
        var importedPrograms = 0;

        foreach (var program in ChuongTrinhCatalog.Programs)
        {
            var current = existingPrograms.FirstOrDefault(item =>
                string.Equals(item.MaChuongTrinh, program.MaChuongTrinh, StringComparison.OrdinalIgnoreCase));

            if (current == null)
            {
                current = new ChuongTrinhDaoTao { MaChuongTrinh = program.MaChuongTrinh };
                _db.ChuongTrinhDaoTaos.Add(current);
                existingPrograms.Add(current);
                importedPrograms++;
            }

            current.TenChuongTrinh = program.TenChuongTrinh;
            current.LoaiChuongTrinh = program.LoaiChuongTrinh;
            current.KhoaQuanLy = program.KhoaQuanLy;
            current.KhoaChuyenMon = program.KhoaChuyenMon;
            current.MaNganhGoc = program.MaNganhGoc;
            current.HinhThucLienKet = program.HinhThucLienKet;
            current.ApDungTuKhoa = program.ApDungTuKhoa;
            current.TichHopCuNhanKySu = program.TichHopCuNhanKySu;
            current.GhiChu = program.GhiChu;
            current.NganhId = program.MaNganhGoc != null &&
                              majorsByCode.TryGetValue(program.MaNganhGoc, out var major)
                ? major.Id
                : null;
        }

        await _db.SaveChangesAsync();
        TempData["Msg"] = $"Đã đồng bộ danh mục: {NganhCatalog.Programs.Count} ngành, " +
                          $"{ChuongTrinhCatalog.Programs.Count} chương trình (thêm mới {imported} ngành, " +
                          $"{importedPrograms} chương trình). Lớp cụ thể cần được tạo riêng.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueCodeAsync(NganhFormViewModel model, int? exceptId = null)
    {
        if (await _db.Nganhs.AnyAsync(nganh =>
                nganh.MaNganh == model.MaNganh && (!exceptId.HasValue || nganh.Id != exceptId.Value)))
        {
            ModelState.AddModelError(nameof(model.MaNganh), "Mã ngành đã tồn tại.");
        }
    }

    private void ValidateFaculty(NganhFormViewModel model)
    {
        if (!NganhCatalog.Faculties.Contains(model.TenKhoa, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.TenKhoa), "Vui lòng chọn một trong 7 khoa hiện hành.");
        }
    }

    private static void Normalize(NganhFormViewModel model)
    {
        model.MaNganh = model.MaNganh?.Trim() ?? "";
        model.TenNganh = model.TenNganh?.Trim() ?? "";
        model.TenKhoa = model.TenKhoa?.Trim() ?? "";
    }

    private async Task SynchronizeFacultyFromDonViAsync(NganhFormViewModel model)
    {
        if (model.DonViId.HasValue && string.IsNullOrWhiteSpace(model.TenKhoa))
        {
            var dv = await _db.DonVis.FindAsync(model.DonViId.Value);
            if (dv != null)
            {
                model.TenKhoa = dv.TenDonVi;
            }
        }
    }

    private async Task LoadDonViDropdownAsync(int? selectedId = null)
    {
        var units = await _db.DonVis.AsNoTracking().Where(d => d.TrangThai == "ACTIVE").OrderBy(d => d.TenDonVi).ToListAsync();
        ViewBag.DonViList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, nameof(DonVi.DonViId), nameof(DonVi.TenDonVi), selectedId);
    }
}
