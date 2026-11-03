using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class QuanLyLopHocPhanController : Controller
{
    private readonly ApplicationDbContext _db;

    public QuanLyLopHocPhanController(ApplicationDbContext db)
    {
        _db = db;
    }

    // 1. Danh sách lớp học phần
    public async Task<IActionResult> Index(string? tuKhoa, int? monHocId, int? giangVienId, string? trangThai, int page = 1)
    {
        const int pageSize = 10;
        page = Math.Max(page, 1);

        var query = _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy)
            .Include(l => l.DangKyHocPhans)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(l => l.MaLopHocPhan.Contains(keyword) || l.TenLopHocPhan.Contains(keyword));
        }

        if (monHocId.HasValue && monHocId > 0)
        {
            query = query.Where(l => l.MonHocId == monHocId.Value);
        }

        if (giangVienId.HasValue && giangVienId > 0)
        {
            query = query.Where(l => l.GiangVienId == giangVienId.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(l => l.TrangThai == trangThai);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page > Math.Max(totalPages, 1)) page = Math.Max(totalPages, 1);

        var rawList = await query
            .OrderBy(l => l.MaLopHocPhan)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rawList.Select(l => new LopHocPhanListItemViewModel
        {
            Id = l.Id,
            MaLopHocPhan = l.MaLopHocPhan,
            TenLopHocPhan = l.TenLopHocPhan,
            TenMon = l.MonHoc?.TenMon ?? "",
            MaMon = l.MonHoc?.MaMon ?? "",
            SoTinChi = l.MonHoc?.SoTinChi ?? 0,
            TenGiangVien = l.GiangVien?.HoTen,
            TenHocKy = l.HocKy?.TenHocKy,
            Nhom = l.Nhom,
            SiSoToiDa = l.SiSoToiDa,
            SiSoHienTai = l.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED"),
            TrangThai = l.TrangThai,
            LichHoc = $"{l.Thu}, Tiết {l.TietBatDau} - {l.TietBatDau + l.SoTiet - 1}",
            PhongHoc = l.PhongHoc
        }).ToList();

        await LoadFilterDropdownsAsync(monHocId, giangVienId);
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.TrangThai = trangThai;

        var result = new PagedResult<LopHocPhanListItemViewModel>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View(result);
    }

    // 2. Tạo mới lớp học phần (GET)
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadFormDropdownsAsync();
        return View("Form", new LopHocPhanFormViewModel
        {
            TrangThai = "OPEN",
            SiSoToiDa = 60,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3,
            PhongHoc = "P.302-A1"
        });
    }

    // 3. Tạo mới lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LopHocPhanFormViewModel model)
    {
        model.MaLopHocPhan = model.MaLopHocPhan.Trim();
        model.TenLopHocPhan = model.TenLopHocPhan.Trim();

        if (await _db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == model.MaLopHocPhan))
        {
            ModelState.AddModelError(nameof(model.MaLopHocPhan), "Mã lớp học phần này đã tồn tại.");
        }

        // Kiểm tra giảng viên bị trùng lịch
        if (model.GiangVienId.HasValue && model.GiangVienId > 0)
        {
            var tietKetThuc = model.TietBatDau + model.SoTiet - 1;
            var trungLich = await _db.LopHocPhans.AnyAsync(l =>
                l.GiangVienId == model.GiangVienId &&
                l.Thu == model.Thu &&
                l.TrangThai == "OPEN" &&
                ((model.TietBatDau >= l.TietBatDau && model.TietBatDau < l.TietBatDau + l.SoTiet) ||
                 (tietKetThuc > l.TietBatDau && tietKetThuc <= l.TietBatDau + l.SoTiet)));

            if (trungLich)
            {
                ModelState.AddModelError(nameof(model.GiangVienId), "Giảng viên này đã có lịch giảng dạy lớp khác vào khung giờ này.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDropdownsAsync(model.MonHocId, model.HocKyId, model.GiangVienId);
            return View("Form", model);
        }

        var lhp = new LopHocPhan
        {
            MaLopHocPhan = model.MaLopHocPhan,
            TenLopHocPhan = model.TenLopHocPhan,
            MonHocId = model.MonHocId,
            HocKyId = model.HocKyId,
            GiangVienId = model.GiangVienId,
            Nhom = model.Nhom,
            SiSoToiDa = model.SiSoToiDa,
            TrangThai = model.TrangThai,
            Thu = model.Thu,
            TietBatDau = model.TietBatDau,
            SoTiet = model.SoTiet,
            PhongHoc = model.PhongHoc,
            GhiChu = model.GhiChu
        };

        _db.LopHocPhans.Add(lhp);
        await _db.SaveChangesAsync();

        TempData["Msg"] = $"Đã tạo lớp học phần '{lhp.MaLopHocPhan}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // 4. Chỉnh sửa lớp học phần (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lhp = await _db.LopHocPhans.FindAsync(id);
        if (lhp == null) return NotFound();

        await LoadFormDropdownsAsync(lhp.MonHocId, lhp.HocKyId, lhp.GiangVienId);
        return View("Form", new LopHocPhanFormViewModel
        {
            Id = lhp.Id,
            MaLopHocPhan = lhp.MaLopHocPhan,
            TenLopHocPhan = lhp.TenLopHocPhan,
            MonHocId = lhp.MonHocId,
            HocKyId = lhp.HocKyId,
            GiangVienId = lhp.GiangVienId,
            Nhom = lhp.Nhom,
            SiSoToiDa = lhp.SiSoToiDa,
            TrangThai = lhp.TrangThai,
            Thu = lhp.Thu,
            TietBatDau = lhp.TietBatDau,
            SoTiet = lhp.SoTiet,
            PhongHoc = lhp.PhongHoc,
            GhiChu = lhp.GhiChu
        });
    }

    // 5. Chỉnh sửa lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LopHocPhanFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        var lhp = await _db.LopHocPhans.FindAsync(id);
        if (lhp == null) return NotFound();

        model.MaLopHocPhan = model.MaLopHocPhan.Trim();
        model.TenLopHocPhan = model.TenLopHocPhan.Trim();

        if (await _db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == model.MaLopHocPhan && l.Id != id))
        {
            ModelState.AddModelError(nameof(model.MaLopHocPhan), "Mã lớp học phần này đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDropdownsAsync(model.MonHocId, model.HocKyId, model.GiangVienId);
            return View("Form", model);
        }

        lhp.MaLopHocPhan = model.MaLopHocPhan;
        lhp.TenLopHocPhan = model.TenLopHocPhan;
        lhp.MonHocId = model.MonHocId;
        lhp.HocKyId = model.HocKyId;
        lhp.GiangVienId = model.GiangVienId;
        lhp.Nhom = model.Nhom;
        lhp.SiSoToiDa = model.SiSoToiDa;
        lhp.TrangThai = model.TrangThai;
        lhp.Thu = model.Thu;
        lhp.TietBatDau = model.TietBatDau;
        lhp.SoTiet = model.SoTiet;
        lhp.PhongHoc = model.PhongHoc;
        lhp.GhiChu = model.GhiChu;

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã cập nhật lớp học phần thành công.";
        return RedirectToAction(nameof(Index));
    }

    // 6. Xem danh sách sinh viên đăng ký vào lớp học phần
    public async Task<IActionResult> SinhVien(int id)
    {
        var lhp = await _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy)
            .Include(l => l.DangKyHocPhans)
                .ThenInclude(dk => dk.SinhVien)
                    .ThenInclude(sv => sv.Lop)
                        .ThenInclude(lop => lop!.Nganh)
            .Include(l => l.Diems)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lhp == null) return NotFound();

        var danhSach = lhp.DangKyHocPhans.Select(dk =>
        {
            var diem = lhp.Diems.FirstOrDefault(d => d.SinhVienId == dk.SinhVienId);
            return new DangKyHocPhanItemViewModel
            {
                DangKyId = dk.Id,
                SinhVienId = dk.SinhVienId,
                MaSinhVien = dk.SinhVien.MaSinhVien,
                HoTen = dk.SinhVien.HoTen,
                TenLopHanhChinh = dk.SinhVien.Lop?.TenLop,
                TenNganh = dk.SinhVien.Lop?.Nganh?.TenNganh,
                NgayDangKy = dk.NgayDangKy,
                LanHoc = dk.LanHoc,
                TrangThai = dk.TrangThai,
                DiemTongKet = diem?.DiemTongKet,
                DiemChu = diem?.DiemChu
            };
        }).OrderBy(s => s.HoTen).ToList();

        // Nạp danh sách sinh viên toàn trường để phục vụ thêm sinh viên vào lớp
        var registeredIds = lhp.DangKyHocPhans.Select(dk => dk.SinhVienId).ToList();
        var eligibleStudents = await _db.SinhViens
            .Include(s => s.Lop)
            .Where(s => !registeredIds.Contains(s.Id))
            .OrderBy(s => s.HoTen)
            .Select(s => new
            {
                s.Id,
                Text = $"{s.MaSinhVien} - {s.HoTen} (Lớp: {s.Lop!.TenLop})"
            })
            .ToListAsync();

        ViewBag.EligibleStudents = new SelectList(eligibleStudents, "Id", "Text");

        var vm = new LopHocPhanSinhVienViewModel
        {
            LopHocPhan = lhp,
            DanhSachSinhVien = danhSach
        };

        return View(vm);
    }

    // 7. Thêm sinh viên vào lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemSinhVien(int lopHocPhanId, int sinhVienId)
    {
        var lhp = await _db.LopHocPhans
            .Include(l => l.DangKyHocPhans)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null) return NotFound();

        var count = lhp.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED");
        if (count >= lhp.SiSoToiDa)
        {
            TempData["Error"] = "Lớp học phần đã đạt sĩ số tối đa.";
            return RedirectToAction(nameof(SinhVien), new { id = lopHocPhanId });
        }

        var exists = await _db.DangKyHocPhans.AnyAsync(dk => dk.LopHocPhanId == lopHocPhanId && dk.SinhVienId == sinhVienId);
        if (exists)
        {
            TempData["Error"] = "Sinh viên này đã có trong danh sách đăng ký của lớp học phần.";
            return RedirectToAction(nameof(SinhVien), new { id = lopHocPhanId });
        }

        var dkMoi = new DangKyHocPhan
        {
            LopHocPhanId = lopHocPhanId,
            SinhVienId = sinhVienId,
            NgayDangKy = DateTime.UtcNow,
            TrangThai = "ENROLLED"
        };

        _db.DangKyHocPhans.Add(dkMoi);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm sinh viên vào lớp học phần thành công.";
        return RedirectToAction(nameof(SinhVien), new { id = lopHocPhanId });
    }

    // 8. Xóa sinh viên khỏi lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaSinhVien(int lopHocPhanId, int sinhVienId)
    {
        var dk = await _db.DangKyHocPhans.FirstOrDefaultAsync(d => d.LopHocPhanId == lopHocPhanId && d.SinhVienId == sinhVienId);
        if (dk == null) return NotFound();

        // Kiểm tra xem đã có điểm thi chưa
        var coDiem = await _db.Diems.AnyAsync(d => d.LopHocPhanId == lopHocPhanId && d.SinhVienId == sinhVienId);
        if (coDiem)
        {
            TempData["Error"] = "Không thể hủy sinh viên đã có điểm tổng kết trong lớp học phần này.";
            return RedirectToAction(nameof(SinhVien), new { id = lopHocPhanId });
        }

        _db.DangKyHocPhans.Remove(dk);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã xóa sinh viên khỏi lớp học phần.";
        return RedirectToAction(nameof(SinhVien), new { id = lopHocPhanId });
    }

    // 9. Xóa lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lhp = await _db.LopHocPhans
            .Include(l => l.DangKyHocPhans)
            .Include(l => l.Diems)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lhp == null) return NotFound();

        if (lhp.DangKyHocPhans.Count > 0)
        {
            TempData["Error"] = "Không thể xóa lớp học phần đang có sinh viên đăng ký. Hãy xóa sinh viên trước.";
            return RedirectToAction(nameof(Index));
        }

        if (lhp.Diems.Count > 0)
        {
            TempData["Error"] = "Không thể xóa lớp học phần đã có bảng điểm học phần.";
            return RedirectToAction(nameof(Index));
        }

        _db.LopHocPhans.Remove(lhp);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã xóa lớp học phần.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFilterDropdownsAsync(int? monHocId = null, int? giangVienId = null)
    {
        ViewBag.DanhSachMonHoc = new SelectList(
            await _db.MonHocs.OrderBy(m => m.TenMon).ToListAsync(),
            "Id", "TenMon", monHocId);

        ViewBag.DanhSachGiangVien = new SelectList(
            await _db.GiangViens.OrderBy(g => g.HoTen).ToListAsync(),
            "Id", "HoTen", giangVienId);
    }

    private async Task LoadFormDropdownsAsync(int? monHocId = null, int? hocKyId = null, int? giangVienId = null)
    {
        ViewBag.DanhSachMonHoc = new SelectList(
            await _db.MonHocs.OrderBy(m => m.TenMon).ToListAsync(),
            "Id", "TenMon", monHocId);

        ViewBag.DanhSachHocKy = new SelectList(
            await _db.HocKies.OrderByDescending(h => h.NgayBatDau).ToListAsync(),
            "HocKyId", "TenHocKy", hocKyId);

        ViewBag.DanhSachGiangVien = new SelectList(
            await _db.GiangViens.OrderBy(g => g.HoTen).ToListAsync(),
            "Id", "HoTen", giangVienId);
    }
}
