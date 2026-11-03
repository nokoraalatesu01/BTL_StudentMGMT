using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV,GiangVien")]
public class BangDiemController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BangDiemController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // 1. Xem danh sách bảng điểm sinh viên (Hỗ trợ lọc theo Lớp học phần & Sinh viên)
    public async Task<IActionResult> Index(int? lopHocPhanId, int? sinhVienId)
    {
        var giangVien = await GetCurrentGiangVienAsync();
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        var query = _db.Diems
            .Include(d => d.SinhVien)
                .ThenInclude(s => s!.Lop)
            .Include(d => d.MonHoc)
            .Include(d => d.LopHocPhan)
            .AsQueryable();

        // Nếu là Giảng viên: Chỉ cho phép xem điểm các Lớp học phần do mình phụ trách
        if (isGiangVienOnly && giangVien != null)
        {
            query = query.Where(d => d.LopHocPhan != null && d.LopHocPhan.GiangVienId == giangVien.Id);
        }

        if (lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
        {
            query = query.Where(d => d.LopHocPhanId == lopHocPhanId.Value);
        }

        if (sinhVienId.HasValue && sinhVienId.Value > 0)
        {
            query = query.Where(d => d.SinhVienId == sinhVienId.Value);
        }

        var list = await query
            .OrderBy(d => d.LopHocPhan != null ? d.LopHocPhan.MaLopHocPhan : "")
            .ThenBy(d => d.SinhVien!.HoTen)
            .ToListAsync();

        await LoadDropdownsAsync(isGiangVienOnly, giangVien?.Id, lopHocPhanId, sinhVienId);
        ViewBag.LopHocPhanId = lopHocPhanId;
        ViewBag.SinhVienId = sinhVienId;

        return View(list);
    }

    // 2. Nhập điểm môn học theo Lớp học phần (GET)
    [HttpGet]
    public async Task<IActionResult> NhapDiem(int? lopHocPhanId, int? sinhVienId, int? monHocId)
    {
        var giangVien = await GetCurrentGiangVienAsync();
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        // Kiểm tra bảo mật IDOR: Giảng viên chỉ được nhập điểm cho lớp học phần mình phụ trách
        if (isGiangVienOnly && giangVien != null && lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
        {
            var isTeacherOfLhp = await _db.LopHocPhans.AnyAsync(l => l.Id == lopHocPhanId.Value && l.GiangVienId == giangVien.Id);
            if (!isTeacherOfLhp)
            {
                TempData["Error"] = "Bạn không có quyền nhập điểm cho lớp học phần của giảng viên khác!";
                return RedirectToAction(nameof(Index));
            }
        }

        await LoadDropdownsAsync(isGiangVienOnly, giangVien?.Id, lopHocPhanId, sinhVienId, monHocId);

        var model = new DiemFormViewModel
        {
            LopHocPhanId = lopHocPhanId,
            SinhVienId = sinhVienId ?? 0,
            MonHocId = monHocId ?? 0
        };

        if (lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
        {
            var lhp = await _db.LopHocPhans.FindAsync(lopHocPhanId.Value);
            if (lhp != null)
            {
                model.MonHocId = lhp.MonHocId;
                model.TenLopHocPhan = lhp.TenLopHocPhan;
            }
        }

        // Nếu đã có điểm thì load lại
        if (sinhVienId > 0 && (lopHocPhanId > 0 || monHocId > 0))
        {
            var query = _db.Diems.AsNoTracking().Where(d => d.SinhVienId == sinhVienId.Value);
            if (lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
            {
                query = query.Where(d => d.LopHocPhanId == lopHocPhanId.Value);
            }
            else if (monHocId.HasValue && monHocId.Value > 0)
            {
                query = query.Where(d => d.MonHocId == monHocId.Value);
            }

            var existing = await query.FirstOrDefaultAsync();
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
        var giangVien = await GetCurrentGiangVienAsync();
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        if (isGiangVienOnly && giangVien != null && model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0)
        {
            var isTeacherOfLhp = await _db.LopHocPhans.AnyAsync(l => l.Id == model.LopHocPhanId.Value && l.GiangVienId == giangVien.Id);
            if (!isTeacherOfLhp)
            {
                TempData["Error"] = "Bạn không có quyền nhập điểm cho lớp học phần này!";
                return RedirectToAction(nameof(Index));
            }
        }

        if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0 && model.MonHocId <= 0)
        {
            var lhp = await _db.LopHocPhans.FindAsync(model.LopHocPhanId.Value);
            if (lhp != null) model.MonHocId = lhp.MonHocId;
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(isGiangVienOnly, giangVien?.Id, model.LopHocPhanId, model.SinhVienId, model.MonHocId);
            return View(model);
        }

        // Tìm bản ghi điểm đã có
        Diem? diem = null;
        if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0)
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d =>
                d.SinhVienId == model.SinhVienId && d.LopHocPhanId == model.LopHocPhanId.Value);
        }
        else
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d =>
                d.SinhVienId == model.SinhVienId && d.MonHocId == model.MonHocId);
        }

        if (diem != null)
        {
            diem.DiemChuyenCan = model.DiemChuyenCan!.Value;
            diem.DiemGiuaKy = model.DiemGiuaKy!.Value;
            diem.DiemCuoiKy = model.DiemCuoiKy!.Value;
            if (model.LopHocPhanId.HasValue) diem.LopHocPhanId = model.LopHocPhanId.Value;
            TempData["Msg"] = "Đã cập nhật điểm học phần thành công!";
        }
        else
        {
            _db.Diems.Add(new Diem
            {
                SinhVienId = model.SinhVienId,
                MonHocId = model.MonHocId,
                LopHocPhanId = model.LopHocPhanId,
                DiemChuyenCan = model.DiemChuyenCan!.Value,
                DiemGiuaKy = model.DiemGiuaKy!.Value,
                DiemCuoiKy = model.DiemCuoiKy!.Value
            });
            TempData["Msg"] = "Đã ghi nhận điểm học phần thành công!";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new
        {
            lopHocPhanId = model.LopHocPhanId,
            sinhVienId = model.SinhVienId
        });
    }

    private async Task<GiangVien?> GetCurrentGiangVienAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _db.GiangViens.FirstOrDefaultAsync(g => g.UserId == user.Id);
    }

    private async Task LoadDropdownsAsync(
        bool isGiangVienOnly,
        int? giangVienId,
        int? selectedLhp = null,
        int? selectedSv = null,
        int? selectedMon = null)
    {
        var queryLhp = _db.LopHocPhans
            .Include(l => l.MonHoc)
            .AsQueryable();

        if (isGiangVienOnly && giangVienId.HasValue)
        {
            queryLhp = queryLhp.Where(l => l.GiangVienId == giangVienId.Value);
        }

        var dsLhp = await queryLhp.OrderBy(l => l.MaLopHocPhan).ToListAsync();

        ViewBag.DanhSachLopHocPhan = new SelectList(
            dsLhp.Select(l => new { l.Id, Text = $"{l.MaLopHocPhan} - {l.TenLopHocPhan}" }),
            "Id", "Text", selectedLhp);

        // Danh sách sinh viên: Nếu có chọn Lớp học phần thì lấy sinh viên đăng ký lớp đó
        if (selectedLhp.HasValue && selectedLhp.Value > 0)
        {
            var svTrongLhp = await _db.DangKyHocPhans
                .Include(dk => dk.SinhVien)
                    .ThenInclude(s => s.Lop)
                .Where(dk => dk.LopHocPhanId == selectedLhp.Value && dk.TrangThai == "ENROLLED")
                .OrderBy(dk => dk.SinhVien.HoTen)
                .Select(dk => new
                {
                    dk.SinhVien.Id,
                    Text = $"{dk.SinhVien.MaSinhVien} - {dk.SinhVien.HoTen} (Lớp: {dk.SinhVien.Lop!.TenLop})"
                })
                .ToListAsync();

            ViewBag.DanhSachSinhVien = new SelectList(svTrongLhp, "Id", "Text", selectedSv);
        }
        else
        {
            var dsSv = await _db.SinhViens
                .Include(s => s.Lop)
                .OrderBy(s => s.HoTen)
                .Select(s => new
                {
                    s.Id,
                    Text = $"{s.MaSinhVien} - {s.HoTen} (Lớp: {s.Lop!.TenLop})"
                })
                .ToListAsync();

            ViewBag.DanhSachSinhVien = new SelectList(dsSv, "Id", "Text", selectedSv);
        }

        ViewBag.DanhSachMonHoc = new SelectList(
            await _db.MonHocs.OrderBy(m => m.TenMon).ToListAsync(),
            "Id", "TenMon", selectedMon);
    }
}
