using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,GiangVien")]
public class BangDiemController : Controller
{
    private readonly ApplicationDbContext _db;

    public BangDiemController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int? sinhVienId)
    {
        var query = _db.Diems
            .Include(d => d.SinhVien)
            .Include(d => d.MonHoc)
            .AsQueryable();

        if (sinhVienId.HasValue && sinhVienId.Value > 0)
        {
            query = query.Where(d => d.SinhVienId == sinhVienId.Value);
        }

        var list = await query
            .OrderBy(d => d.SinhVien!.HoTen)
            .ThenBy(d => d.MonHoc!.TenMon)
            .ToListAsync();

        ViewBag.SinhVienId = sinhVienId;
        ViewBag.DanhSachSinhVien = new SelectList(
            await _db.SinhViens.OrderBy(s => s.HoTen).ToListAsync(),
            "Id",
            "HoTen",
            sinhVienId);

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> NhapDiem(int? sinhVienId)
    {
        await LoadDropdownsAsync(sinhVienId);
        return View(new DiemFormViewModel
        {
            SinhVienId = sinhVienId ?? 0,
            DiemChuyenCan = 10,
            DiemGiuaKy = 8,
            DiemCuoiKy = 8.5
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapDiem(DiemFormViewModel model)
    {
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
            // Cập nhật điểm
            diem.DiemChuyenCan = model.DiemChuyenCan;
            diem.DiemGiuaKy = model.DiemGiuaKy;
            diem.DiemCuoiKy = model.DiemCuoiKy;
            TempData["Msg"] = "Đã cập nhật điểm học phần thành công!";
        }
        else
        {
            // Thêm mới
            dbAddDiem:
            _db.Diems.Add(new Diem
            {
                SinhVienId = model.SinhVienId,
                MonHocId = model.MonHocId,
                DiemChuyenCan = model.DiemChuyenCan,
                DiemGiuaKy = model.DiemGiuaKy,
                DiemCuoiKy = model.DiemCuoiKy
            });
            TempData["Msg"] = "Đã ghi nhận điểm học phần thành công!";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { sinhVienId = model.SinhVienId });
    }

    private async Task LoadDropdownsAsync(int? selectedSinhVien = null, int? selectedMonHoc = null)
    {
        ViewBag.DanhSachSinhVien = new SelectList(
            await _db.SinhViens.OrderBy(s => s.HoTen).ToListAsync(),
            "Id",
            "HoTen",
            selectedSinhVien);

        ViewBag.DanhSachMonHoc = new SelectList(
            await _db.MonHocs.OrderBy(m => m.TenMon).ToListAsync(),
            "Id",
            "TenMon",
            selectedMonHoc);
    }
}
