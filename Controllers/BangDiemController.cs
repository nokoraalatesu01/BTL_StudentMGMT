using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV,GiangVien")]
public class BangDiemController : Controller
{
    private readonly ApplicationDbContext _db;

    public BangDiemController(ApplicationDbContext db)
    {
        _db = db;
    }

    // 1. Xem danh sách bảng điểm sinh viên
    public async Task<IActionResult> Index(int? sinhVienId)
    {
        var query = _db.Diems
            .AsNoTracking()
            .Include(d => d.SinhVien)
            .Include(d => d.MonHoc)
            .AsQueryable();

        if (sinhVienId.HasValue && sinhVienId.Value > 0)
        {
            query = query.Where(d => d.SinhVienId == sinhVienId.Value);
        }

        var list = await query
            .OrderBy(d => d.SinhVien!.MaSinhVien)
            .ThenBy(d => d.MonHoc!.TenMon)
            .ToListAsync();

        await LoadDropdownsAsync(sinhVienId);
        ViewBag.SinhVienId = sinhVienId;

        return View(list);
    }

    // 2. Nhập điểm môn học (GET)
    [HttpGet]
    public async Task<IActionResult> NhapDiem(int? sinhVienId, int? monHocId)
    {
        await LoadDropdownsAsync(sinhVienId, monHocId);

        var model = new DiemFormViewModel
        {
            SinhVienId = sinhVienId ?? 0,
            MonHocId = monHocId ?? 0
        };

        // Nếu đã có kết quả nhập trước đó thì nạp lại giá trị cũ để chỉnh sửa.
        if (sinhVienId > 0 && monHocId > 0)
        {
            var existing = await _db.Diems
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.SinhVienId == sinhVienId && d.MonHocId == monHocId);

            if (existing != null)
            {
                model.Id = existing.Id;
                model.DiemChuyenCan = existing.DiemChuyenCan;
                model.DiemGiuaKy = existing.DiemGiuaKy;
                model.DiemCuoiKy = existing.DiemCuoiKy;
            }
        }

        return View(model);
    }

    // 3. Nhập điểm môn học (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapDiem(DiemFormViewModel model)
    {
        if (model.SinhVienId <= 0 || !await _db.SinhViens.AnyAsync(s => s.Id == model.SinhVienId))
        {
            ModelState.AddModelError(nameof(model.SinhVienId), "Sinh viên được chọn không tồn tại trong hệ thống.");
        }

        if (model.MonHocId <= 0 || !await _db.MonHocs.AnyAsync(m => m.Id == model.MonHocId))
        {
            ModelState.AddModelError(nameof(model.MonHocId), "Môn học được chọn không tồn tại trong hệ thống.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(model.SinhVienId, model.MonHocId);
            return View(model);
        }

        // Kiểm tra xem sinh viên đã có điểm môn này chưa
        var diem = await _db.Diems.FirstOrDefaultAsync(d =>
            d.SinhVienId == model.SinhVienId && d.MonHocId == model.MonHocId);

        if (diem != null)
        {
            // Cập nhật lại điểm
            diem.DiemChuyenCan = model.DiemChuyenCan!.Value;
            diem.DiemGiuaKy = model.DiemGiuaKy!.Value;
            diem.DiemCuoiKy = model.DiemCuoiKy!.Value;
            TempData["Msg"] = "Đã cập nhật điểm học phần thành công!";
        }
        else
        {
            // Thêm mới kết quả điểm
            _db.Diems.Add(new Diem
            {
                SinhVienId = model.SinhVienId,
                MonHocId = model.MonHocId,
                DiemChuyenCan = model.DiemChuyenCan!.Value,
                DiemGiuaKy = model.DiemGiuaKy!.Value,
                DiemCuoiKy = model.DiemCuoiKy!.Value
            });
            TempData["Msg"] = "Đã ghi nhận điểm học phần thành công!";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { sinhVienId = model.SinhVienId });
    }

    // Hàm phụ nạp dữ liệu DropDownList
    private async Task LoadDropdownsAsync(int? selectedSinhVien = null, int? selectedMonHoc = null)
    {
        var sinhViens = await _db.SinhViens
            .AsNoTracking()
            .OrderBy(s => s.MaSinhVien)
            .Select(s => new { s.Id, Display = s.MaSinhVien + " - " + s.HoTen })
            .ToListAsync();

        var monHocs = await _db.MonHocs
            .AsNoTracking()
            .OrderBy(m => m.MaMon)
            .Select(m => new { m.Id, Display = m.MaMon + " - " + m.TenMon })
            .ToListAsync();

        ViewBag.DanhSachSinhVien = new SelectList(
            sinhViens,
            "Id",
            "Display",
            selectedSinhVien);

        ViewBag.DanhSachMonHoc = new SelectList(
            monHocs,
            "Id",
            "Display",
            selectedMonHoc);
    }
}
