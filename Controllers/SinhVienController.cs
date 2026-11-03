using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "SinhVien")]
public class SinhVienController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;

    public SinhVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService)
    {
        _db = db;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await LoadCurrentStudentAsync();
        return model == null ? View("Unlinked") : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatAnhDaiDien(IFormFile? fileAnh)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction(nameof(HoSo));

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return RedirectToAction(nameof(HoSo));

        if (fileAnh != null && fileAnh.Length > 0)
        {
            var avatarPath = await _fileUploadService.UploadAvatarAsync(fileAnh, student.HinhAnh);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                student.HinhAnh = avatarPath;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
            }
            else
            {
                TempData["Error"] = "Định dạng file không hợp lệ (.jpg, .jpeg, .png, .webp) hoặc vượt quá 5MB.";
            }
        }
        return RedirectToAction(nameof(HoSo));
    }

    public async Task<IActionResult> HoSo()
    {
        var model = await LoadCurrentStudentAsync();
        return model == null ? View("Unlinked") : View(model);
    }

    public async Task<IActionResult> BangDiem()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return View("Unlinked");
        }

        var student = await _db.SinhViens
            .AsNoTracking()
            .Include(item => item.Lop)
                .ThenInclude(lop => lop!.Nganh)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.MonHoc)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.LopHocPhan)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (student == null)
        {
            return View("Unlinked");
        }

        var model = new SinhVienDiemViewModel
        {
            MaSinhVien = student.MaSinhVien,
            HoTen = student.HoTen,
            TenLop = student.Lop?.TenLop,
            TenNganh = student.Lop?.Nganh?.TenNganh,
            NienKhoa = student.NienKhoa,
            TrangThai = student.TrangThai,
            HinhAnh = student.HinhAnh,
            Diems = student.Diems
                .OrderBy(diem => diem.MonHoc?.TenMon)
                .ToList()
        };

        return View(model);
    }

    // 4. Màn hình đăng ký học phần (Tín chỉ)
    public async Task<IActionResult> DangKyHocPhan()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return View("Unlinked");

        var student = await _db.SinhViens
            .Include(s => s.Lop)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null) return View("Unlinked");

        // Lấy các lớp học phần đang mở đăng ký
        var lhpList = await _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.HocKy)
            .Include(l => l.DangKyHocPhans)
            .Where(l => l.TrangThai == "OPEN")
            .OrderBy(l => l.MonHoc!.TenMon)
            .ThenBy(l => l.Nhom)
            .ToListAsync();

        var registeredList = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
                .ThenInclude(l => l.MonHoc)
            .Include(dk => dk.LopHocPhan)
                .ThenInclude(l => l.GiangVien)
            .Where(dk => dk.SinhVienId == student.Id && dk.TrangThai == "ENROLLED")
            .ToListAsync();

        var registeredLhpIds = registeredList.Select(dk => dk.LopHocPhanId).ToHashSet();

        var vm = new SinhVienDangKyHocPhanViewModel
        {
            SinhVien = student,
            LopHocPhansMo = lhpList.Select(l => new LopHocPhanDangKyItemViewModel
            {
                LopHocPhanId = l.Id,
                MaLopHocPhan = l.MaLopHocPhan,
                TenLopHocPhan = l.TenLopHocPhan,
                MaMon = l.MonHoc?.MaMon ?? "",
                TenMon = l.MonHoc?.TenMon ?? "",
                SoTinChi = l.MonHoc?.SoTinChi ?? 0,
                TenGiangVien = l.GiangVien?.HoTen,
                LichHoc = $"{l.Thu}, Tiết {l.TietBatDau} - {l.TietBatDau + l.SoTiet - 1}",
                PhongHoc = l.PhongHoc,
                SiSoHienTai = l.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED"),
                SiSoToiDa = l.SiSoToiDa,
                DaDangKy = registeredLhpIds.Contains(l.Id)
            }).ToList(),
            LopHocPhanDaDangKy = registeredList.Select(dk => new LopHocPhanDangKyItemViewModel
            {
                LopHocPhanId = dk.LopHocPhanId,
                MaLopHocPhan = dk.LopHocPhan.MaLopHocPhan,
                TenLopHocPhan = dk.LopHocPhan.TenLopHocPhan,
                MaMon = dk.LopHocPhan.MonHoc?.MaMon ?? "",
                TenMon = dk.LopHocPhan.MonHoc?.TenMon ?? "",
                SoTinChi = dk.LopHocPhan.MonHoc?.SoTinChi ?? 0,
                TenGiangVien = dk.LopHocPhan.GiangVien?.HoTen,
                LichHoc = $"{dk.LopHocPhan.Thu}, Tiết {dk.LopHocPhan.TietBatDau} - {dk.LopHocPhan.TietBatDau + dk.LopHocPhan.SoTiet - 1}",
                PhongHoc = dk.LopHocPhan.PhongHoc,
                SiSoHienTai = 0,
                SiSoToiDa = dk.LopHocPhan.SiSoToiDa,
                DaDangKy = true,
                NgayDangKy = dk.NgayDangKy
            }).ToList()
        };

        return View(vm);
    }

    // 5. Xác nhận đăng ký lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanDangKy(int lopHocPhanId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction(nameof(DangKyHocPhan));

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return RedirectToAction(nameof(DangKyHocPhan));

        var lhp = await _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.DangKyHocPhans)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId);

        if (lhp == null)
        {
            TempData["Error"] = "Lớp học phần không tồn tại.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        if (lhp.TrangThai != "OPEN")
        {
            TempData["Error"] = "Lớp học phần đã đóng đăng ký hoặc kết thúc.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra đã đăng ký lớp học phần này chưa
        var daDangKy = await _db.DangKyHocPhans.AnyAsync(dk =>
            dk.SinhVienId == student.Id &&
            dk.LopHocPhanId == lopHocPhanId &&
            dk.TrangThai == "ENROLLED");

        if (daDangKy)
        {
            TempData["Error"] = "Bạn đã đăng ký lớp học phần này rồi.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra sĩ số tối đa
        var count = lhp.DangKyHocPhans.Count(dk => dk.TrangThai == "ENROLLED");
        if (count >= lhp.SiSoToiDa)
        {
            TempData["Error"] = "Lớp học phần đã đầy sĩ số (hết chỗ). Vui lòng chọn nhóm lớp khác.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra đã đăng ký nhóm khác của cùng môn học trong cùng học kỳ chưa
        var daDangKyCungMon = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
            .AnyAsync(dk =>
                dk.SinhVienId == student.Id &&
                dk.LopHocPhan.MonHocId == lhp.MonHocId &&
                dk.LopHocPhan.HocKyId == lhp.HocKyId &&
                dk.TrangThai == "ENROLLED");

        if (daDangKyCungMon)
        {
            TempData["Error"] = $"Bạn đã đăng ký một nhóm khác của môn '{lhp.MonHoc?.TenMon}' trong học kỳ này.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Kiểm tra trùng lịch học với các môn đã đăng ký
        var currentEnrollments = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
            .Where(dk => dk.SinhVienId == student.Id && dk.TrangThai == "ENROLLED")
            .Select(dk => dk.LopHocPhan)
            .ToListAsync();

        var tietKetThuc = lhp.TietBatDau + lhp.SoTiet - 1;
        var trungLich = currentEnrollments.FirstOrDefault(other =>
            other.Thu == lhp.Thu &&
            ((lhp.TietBatDau >= other.TietBatDau && lhp.TietBatDau < other.TietBatDau + other.SoTiet) ||
             (tietKetThuc > other.TietBatDau && tietKetThuc <= other.TietBatDau + other.SoTiet)));

        if (trungLich != null)
        {
            TempData["Error"] = $"Trùng lịch học: Lớp này học vào {lhp.Thu} (tiết {lhp.TietBatDau}-{tietKetThuc}), trùng với môn '{trungLich.TenLopHocPhan}'.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        // Thực hiện ghi danh
        var existingRecord = await _db.DangKyHocPhans.FirstOrDefaultAsync(dk =>
            dk.SinhVienId == student.Id && dk.LopHocPhanId == lopHocPhanId);

        if (existingRecord != null)
        {
            existingRecord.TrangThai = "ENROLLED";
            existingRecord.NgayDangKy = DateTime.UtcNow;
        }
        else
        {
            _db.DangKyHocPhans.Add(new DangKyHocPhan
            {
                SinhVienId = student.Id,
                LopHocPhanId = lopHocPhanId,
                NgayDangKy = DateTime.UtcNow,
                TrangThai = "ENROLLED"
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đăng ký thành công lớp học phần '{lhp.TenLopHocPhan}'!";
        return RedirectToAction(nameof(DangKyHocPhan));
    }

    // 6. Hủy đăng ký lớp học phần (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyDangKy(int lopHocPhanId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction(nameof(DangKyHocPhan));

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return RedirectToAction(nameof(DangKyHocPhan));

        var dk = await _db.DangKyHocPhans
            .Include(d => d.LopHocPhan)
            .FirstOrDefaultAsync(d => d.SinhVienId == student.Id && d.LopHocPhanId == lopHocPhanId && d.TrangThai == "ENROLLED");

        if (dk == null)
        {
            TempData["Error"] = "Bản ghi đăng ký không tồn tại.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        if (dk.LopHocPhan.TrangThai != "OPEN")
        {
            TempData["Error"] = "Lớp học phần đã khóa đăng ký, không thể hủy.";
            return RedirectToAction(nameof(DangKyHocPhan));
        }

        _db.DangKyHocPhans.Remove(dk);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã hủy đăng ký lớp học phần '{dk.LopHocPhan.TenLopHocPhan}'.";
        return RedirectToAction(nameof(DangKyHocPhan));
    }

    // 7. Thời khóa biểu cá nhân của sinh viên
    public async Task<IActionResult> ThoiKhoaBieu()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return View("Unlinked");

        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null) return View("Unlinked");

        var registered = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
                .ThenInclude(l => l.MonHoc)
            .Include(dk => dk.LopHocPhan)
                .ThenInclude(l => l.GiangVien)
            .Where(dk => dk.SinhVienId == student.Id && dk.TrangThai == "ENROLLED")
            .Select(dk => new ThoiKhoaBieuItemViewModel
            {
                LopHocPhanId = dk.LopHocPhanId,
                MaLopHocPhan = dk.LopHocPhan.MaLopHocPhan,
                TenMon = dk.LopHocPhan.MonHoc != null ? dk.LopHocPhan.MonHoc.TenMon : dk.LopHocPhan.TenLopHocPhan,
                SoTinChi = dk.LopHocPhan.MonHoc != null ? dk.LopHocPhan.MonHoc.SoTinChi : 0,
                GiangVien = dk.LopHocPhan.GiangVien != null ? dk.LopHocPhan.GiangVien.HoTen : "Chưa phân công",
                Thu = dk.LopHocPhan.Thu,
                TietBatDau = dk.LopHocPhan.TietBatDau,
                SoTiet = dk.LopHocPhan.SoTiet,
                PhongHoc = dk.LopHocPhan.PhongHoc
            })
            .OrderBy(x => x.Thu)
            .ThenBy(x => x.TietBatDau)
            .ToListAsync();

        ViewBag.SinhVien = student;
        return View(registered);
    }

    private async Task<SinhVienPortalViewModel?> LoadCurrentStudentAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return null;
        }

        var student = await _db.SinhViens
            .AsNoTracking()
            .Include(item => item.Lop)
                .ThenInclude(lop => lop!.Nganh)
            .Include(item => item.Diems)
                .ThenInclude(diem => diem.MonHoc)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (student == null)
        {
            return null;
        }

        var classmates = student.Lop == null
            ? (int?)null
            : await _db.SinhViens.CountAsync(item => item.LopId == student.LopId);

        var gpa = student.Gpa;

        return new SinhVienPortalViewModel
        {
            UserName = User.Identity?.Name ?? "",
            HoTen = student.HoTen,
            MaSinhVien = student.MaSinhVien,
            NgaySinh = student.NgaySinh,
            GioiTinh = student.GioiTinh,
            Email = student.Email,
            SoDienThoai = student.SoDienThoai,
            DiaChi = student.DiaChi,
            TenLop = student.Lop?.TenLop,
            MaNganh = student.Lop?.Nganh?.MaNganh,
            TenNganh = student.Lop?.Nganh?.TenNganh,
            TenKhoa = student.Lop?.Nganh?.TenKhoa,
            SiSoLop = classmates,
            NienKhoa = student.NienKhoa,
            TrangThai = student.TrangThai,
            HinhAnh = student.HinhAnh,
            Gpa = gpa,
            XepLoai = student.XepLoai,
            SoMonDaDat = student.Diems.Count(diem => diem.DiemTongKet >= 4.0),
            SoMon = student.Diems.Count
        };
    }
}
