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

    // 1. Xem danh sách bảng điểm sinh viên
    public IActionResult Index(int? sinhVienId)
    {
        var query = _db.Diems
            .Include(d => d.SinhVien)
            .Include(d => d.MonHoc)
            .AsQueryable();

        if (sinhVienId.HasValue && sinhVienId.Value > 0)
        {
            query = query.Where(d => d.SinhVienId == sinhVienId.Value);
        }

        var list = query
            .OrderBy(d => d.SinhVien!.HoTen)
            .ThenBy(d => d.MonHoc!.TenMon)
            .ToList();

        ViewBag.SinhVienId = sinhVienId;
        ViewBag.DanhSachSinhVien = new SelectList(
            _db.SinhViens.OrderBy(s => s.HoTen).ToList(),
            "Id",
            "HoTen",
            sinhVienId);

        return View(list);
    }

    // 2. Nhập điểm môn học (GET)
    [HttpGet]
    public IActionResult NhapDiem(int? sinhVienId, int? monHocId)
    {
        LoadDropdowns(sinhVienId, monHocId);

        var model = new DiemFormViewModel
        {
            SinhVienId = sinhVienId ?? 0,
            MonHocId = monHocId ?? 0
        };

        // Nếu đã có kết quả nhập trước đó thì nạp lại giá trị cũ để chỉnh sửa.
        if (sinhVienId > 0 && monHocId > 0)
        {
            var existing = _db.Diems
                .AsNoTracking()
                .FirstOrDefault(d => d.SinhVienId == sinhVienId && d.MonHocId == monHocId);

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
    public IActionResult NhapDiem(DiemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            LoadDropdowns(model.SinhVienId, model.MonHocId);
            return View(model);
        }

        // Kiểm tra xem sinh viên đã có điểm môn này chưa
        var diem = _db.Diems.FirstOrDefault(d =>
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

        _db.SaveChanges();
        return RedirectToAction(nameof(Index), new { sinhVienId = model.SinhVienId });
    }

    // Hàm phụ nạp dữ liệu DropDownList
    private void LoadDropdowns(int? selectedSinhVien = null, int? selectedMonHoc = null)
    {
        ViewBag.DanhSachSinhVien = new SelectList(
            _db.SinhViens.OrderBy(s => s.HoTen).ToList(),
            "Id",
            "HoTen",
            selectedSinhVien);

        ViewBag.DanhSachMonHoc = new SelectList(
            _db.MonHocs.OrderBy(m => m.TenMon).ToList(),
            "Id",
            "TenMon",
            selectedMonHoc);
    }
}
