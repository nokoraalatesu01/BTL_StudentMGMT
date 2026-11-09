using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class QuanLyLopHocPhanController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAttendanceService _attendanceService;
    private readonly IEnrollmentService _enrollmentService;

    public QuanLyLopHocPhanController(
        ApplicationDbContext db,
        IAttendanceService attendanceService,
        IEnrollmentService enrollmentService)
    {
        _db = db;
        _attendanceService = attendanceService;
        _enrollmentService = enrollmentService;
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
            SiSoHienTai = l.DangKyHocPhans.Count(dk => dk.TrangThai == EnrollmentStatus.Enrolled),
            TrangThai = l.TrangThai,
            LichHoc = $"{l.Thu}, {CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(l.TietBatDau))} ({CaHocHelper.GetKhungGio(l.TietBatDau, l.SoTiet)})",
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
            Ca = 1,
            TietBatDau = 1,
            SoTiet = 2,
            PhongHoc = "P.302-A1"
        });
    }

    // 3. Tạo mới lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LopHocPhanFormViewModel model)
    {
        model.MaLopHocPhan = model.MaLopHocPhan?.Trim() ?? string.Empty;
        model.TenLopHocPhan = model.TenLopHocPhan?.Trim() ?? string.Empty;
        model.PhongHoc = model.PhongHoc?.Trim() ?? string.Empty;

        if (await _db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == model.MaLopHocPhan))
        {
            ModelState.AddModelError(nameof(model.MaLopHocPhan), "Mã lớp học phần này đã tồn tại.");
        }

        // Ánh xạ Ca -> (TietBatDau, SoTiet) từ CaHocHelper
        if (!model.Ca.HasValue || !CaHocHelper.IsValidCa(model.Ca.Value))
        {
            ModelState.AddModelError(nameof(model.Ca), "Ca học không hợp lệ. Vui lòng chọn ca từ Ca 1 đến Ca 5.");
        }
        else
        {
            var (start, count) = CaHocHelper.GetTietTuCa(model.Ca.Value);
            model.TietBatDau = start;
            model.SoTiet = count;
        }

        var tietKetThuc = model.TietBatDau + model.SoTiet - 1;

        // Kiểm tra giảng viên bị trùng lịch: a.Start <= b.End && b.Start <= a.End
        if (model.GiangVienId.HasValue && model.GiangVienId > 0)
        {
            var trungLichGV = await _db.LopHocPhans
                .Where(l => l.GiangVienId == model.GiangVienId &&
                            l.Thu == model.Thu &&
                            l.TrangThai == "OPEN" &&
                            model.TietBatDau <= (l.TietBatDau + l.SoTiet - 1) &&
                            l.TietBatDau <= tietKetThuc)
                .Select(l => new { l.MaLopHocPhan, l.Thu, l.TietBatDau, l.SoTiet })
                .FirstOrDefaultAsync();

            if (trungLichGV != null)
            {
                var caTen = CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(trungLichGV.TietBatDau));
                ModelState.AddModelError(nameof(model.GiangVienId),
                    $"Giảng viên này đã có lịch giảng dạy lớp '{trungLichGV.MaLopHocPhan}' vào {trungLichGV.Thu}, {caTen} ({CaHocHelper.GetKhungGio(trungLichGV.TietBatDau, trungLichGV.SoTiet)}).");
            }
        }

        // Kiểm tra trùng PHÒNG HỌC: cùng PhongHoc, cùng Thu, cùng Ca, cùng HocKyId, trạng thái OPEN, khác lớp
        if (!string.IsNullOrWhiteSpace(model.PhongHoc) && model.HocKyId.HasValue)
        {
            var phongNormalized = model.PhongHoc.Trim();
            var trungPhong = await _db.LopHocPhans
                .Where(l => l.PhongHoc != null && l.PhongHoc.Trim() == phongNormalized &&
                            l.Thu == model.Thu &&
                            l.HocKyId == model.HocKyId &&
                            l.TrangThai == "OPEN" &&
                            model.TietBatDau <= (l.TietBatDau + l.SoTiet - 1) &&
                            l.TietBatDau <= tietKetThuc)
                .Select(l => new { l.MaLopHocPhan, l.Thu, l.TietBatDau, l.SoTiet })
                .FirstOrDefaultAsync();

            if (trungPhong != null)
            {
                var caTen = CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(trungPhong.TietBatDau));
                ModelState.AddModelError(nameof(model.PhongHoc),
                    $"Phòng học '{phongNormalized}' đã được xếp cho lớp '{trungPhong.MaLopHocPhan}' vào {trungPhong.Thu}, {caTen} ({CaHocHelper.GetKhungGio(trungPhong.TietBatDau, trungPhong.SoTiet)}).");
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

        int? caValue = CaHocHelper.GetCaChuan(lhp.TietBatDau, lhp.SoTiet);

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
            Ca = caValue, // null nếu dữ liệu cũ không khớp 1 ca chuẩn
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

        model.MaLopHocPhan = model.MaLopHocPhan?.Trim() ?? string.Empty;
        model.TenLopHocPhan = model.TenLopHocPhan?.Trim() ?? string.Empty;
        model.PhongHoc = model.PhongHoc?.Trim() ?? string.Empty;

        if (await _db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == model.MaLopHocPhan && l.Id != id))
        {
            ModelState.AddModelError(nameof(model.MaLopHocPhan), "Mã lớp học phần này đã tồn tại.");
        }

        // Ánh xạ Ca -> (TietBatDau, SoTiet) từ CaHocHelper
        if (!model.Ca.HasValue || !CaHocHelper.IsValidCa(model.Ca.Value))
        {
            ModelState.AddModelError(nameof(model.Ca), "Ca học không hợp lệ. Vui lòng chọn ca từ Ca 1 đến Ca 5.");
        }
        else
        {
            var (start, count) = CaHocHelper.GetTietTuCa(model.Ca.Value);
            model.TietBatDau = start;
            model.SoTiet = count;
        }

        var tietKetThuc = model.TietBatDau + model.SoTiet - 1;

        // Kiểm tra giảng viên bị trùng lịch (loại trừ chính lớp đang sửa)
        if (model.GiangVienId.HasValue && model.GiangVienId > 0)
        {
            var trungLichGV = await _db.LopHocPhans
                .Where(l => l.Id != id &&
                            l.GiangVienId == model.GiangVienId &&
                            l.Thu == model.Thu &&
                            l.TrangThai == "OPEN" &&
                            model.TietBatDau <= (l.TietBatDau + l.SoTiet - 1) &&
                            l.TietBatDau <= tietKetThuc)
                .Select(l => new { l.MaLopHocPhan, l.Thu, l.TietBatDau, l.SoTiet })
                .FirstOrDefaultAsync();

            if (trungLichGV != null)
            {
                var caTen = CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(trungLichGV.TietBatDau));
                ModelState.AddModelError(nameof(model.GiangVienId),
                    $"Giảng viên này đã có lịch giảng dạy lớp '{trungLichGV.MaLopHocPhan}' vào {trungLichGV.Thu}, {caTen} ({CaHocHelper.GetKhungGio(trungLichGV.TietBatDau, trungLichGV.SoTiet)}).");
            }
        }

        // Kiểm tra trùng PHÒNG HỌC: cùng PhongHoc, cùng Thu, cùng Ca, cùng HocKyId, trạng thái OPEN, khác lớp
        if (!string.IsNullOrWhiteSpace(model.PhongHoc) && model.HocKyId.HasValue)
        {
            var phongNormalized = model.PhongHoc.Trim();
            var trungPhong = await _db.LopHocPhans
                .Where(l => l.Id != id &&
                            l.PhongHoc != null && l.PhongHoc.Trim() == phongNormalized &&
                            l.Thu == model.Thu &&
                            l.HocKyId == model.HocKyId &&
                            l.TrangThai == "OPEN" &&
                            model.TietBatDau <= (l.TietBatDau + l.SoTiet - 1) &&
                            l.TietBatDau <= tietKetThuc)
                .Select(l => new { l.MaLopHocPhan, l.Thu, l.TietBatDau, l.SoTiet })
                .FirstOrDefaultAsync();

            if (trungPhong != null)
            {
                var caTen = CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(trungPhong.TietBatDau));
                ModelState.AddModelError(nameof(model.PhongHoc),
                    $"Phòng học '{phongNormalized}' đã được xếp cho lớp '{trungPhong.MaLopHocPhan}' vào {trungPhong.Thu}, {caTen} ({CaHocHelper.GetKhungGio(trungPhong.TietBatDau, trungPhong.SoTiet)}).");
            }
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

        var danhSach = lhp.DangKyHocPhans
            .Where(dk => dk.TrangThai == EnrollmentStatus.Enrolled)
            .Select(dk =>
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

        // Nạp danh sách sinh viên toàn trường để phục vụ thêm sinh viên vào lớp (chỉ trừ sinh viên đang ENROLLED)
        var registeredIds = lhp.DangKyHocPhans
            .Where(dk => dk.TrangThai == EnrollmentStatus.Enrolled)
            .Select(dk => dk.SinhVienId)
            .ToList();
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
        var result = await _enrollmentService.EnrollAsync(sinhVienId, lopHocPhanId, EnrollmentActor.Admin);
        if (result.Success)
        {
            TempData["Msg"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
        return RedirectToAction(nameof(SinhVien), new { id = lopHocPhanId });
    }

    // 8. Xóa sinh viên khỏi lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaSinhVien(int lopHocPhanId, int sinhVienId)
    {
        var result = await _enrollmentService.DropAsync(sinhVienId, lopHocPhanId, EnrollmentActor.Admin);
        if (result.Success)
        {
            TempData["Msg"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
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

        if (lhp.DangKyHocPhans.Any(dk => dk.TrangThai == EnrollmentStatus.Enrolled))
        {
            TempData["Error"] = "Không thể xóa lớp học phần đang có sinh viên đăng ký. Hãy xóa sinh viên trước.";
            return RedirectToAction(nameof(Index));
        }

        if (lhp.Diems.Count > 0)
        {
            TempData["Error"] = "Không thể xóa lớp học phần đã có bảng điểm học phần.";
            return RedirectToAction(nameof(Index));
        }

        if (lhp.DangKyHocPhans.Count > 0)
        {
            _db.DangKyHocPhans.RemoveRange(lhp.DangKyHocPhans);
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
