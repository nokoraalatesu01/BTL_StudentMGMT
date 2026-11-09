using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
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
    private readonly IGiangVienSecurityService _securityService;

    public BangDiemController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IGiangVienSecurityService securityService)
    {
        _db = db;
        _userManager = userManager;
        _securityService = securityService;
    }

    // 1. Xem danh sách bảng điểm sinh viên (Hỗ trợ lọc theo Lớp học phần, Sinh viên & Học kỳ)
    public async Task<IActionResult> Index(int? lopHocPhanId, int? sinhVienId, int? hocKyId, string? loaiHoc, int? lanThi)
    {
        var giangVien = await _securityService.GetCurrentGiangVienAsync(User);
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        if (isGiangVienOnly && giangVien == null)
        {
            TempData["Error"] = "Tài khoản của bạn chưa được liên kết với hồ sơ Giảng viên.";
            return RedirectToAction("Index", "GiangVien");
        }

        // Kiểm tra bảo mật IDOR nếu giảng viên chủ động truyền lopHocPhanId trên URL
        if (isGiangVienOnly && lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
        {
            var canAccess = await _securityService.CanAccessLopHocPhanAsync(User, lopHocPhanId.Value);
            if (!canAccess)
            {
                _securityService.LogSecurityViolation(User, "BangDiem.Index", $"LopHocPhanId={lopHocPhanId.Value}", "Giảng viên cố tình xem điểm lớp học phần của giảng viên khác.");
                TempData["Error"] = "Bạn không có quyền xem điểm của lớp học phần này!";
                return RedirectToAction(nameof(Index));
            }
        }

        var query = _db.Diems
            .AsNoTracking()
            .Include(d => d.SinhVien)
                .ThenInclude(s => s!.Lop)
            .Include(d => d.MonHoc)
            .Include(d => d.LopHocPhan)
            .Include(d => d.HocKy)
            .AsQueryable();

        // Nếu là Giảng viên: Bắt buộc chỉ xem điểm các Lớp học phần do mình phụ trách
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

        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            query = query.Where(d => d.HocKyId == hocKyId.Value || (d.LopHocPhan != null && d.LopHocPhan.HocKyId == hocKyId.Value));
        }

        if (!string.IsNullOrEmpty(loaiHoc))
        {
            query = query.Where(d => d.LoaiHoc == loaiHoc);
        }

        if (lanThi.HasValue && lanThi.Value > 0)
        {
            query = query.Where(d => d.LanThi == lanThi.Value);
        }

        var list = await query
            .OrderBy(d => d.SinhVien!.HoTen)
            .ThenBy(d => d.MonHoc!.TenMon)
            .ThenBy(d => d.LanHoc)
            .ThenBy(d => d.LanThi)
            .ToListAsync();

        await LoadDropdownsAsync(isGiangVienOnly, giangVien?.Id, lopHocPhanId, sinhVienId, null, hocKyId);
        ViewBag.LopHocPhanId = lopHocPhanId;
        ViewBag.SinhVienId = sinhVienId;
        ViewBag.HocKyId = hocKyId;
        ViewBag.LoaiHoc = loaiHoc;
        ViewBag.LanThi = lanThi;

        return View(list);
    }

    // 2. Nhập điểm môn học theo Lớp học phần (GET)
    [HttpGet]
    public async Task<IActionResult> NhapDiem(int? lopHocPhanId, int? sinhVienId, int? monHocId)
    {
        var giangVien = await _securityService.GetCurrentGiangVienAsync(User);
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        if (isGiangVienOnly)
        {
            if (giangVien == null)
            {
                TempData["Error"] = "Tài khoản của bạn chưa được liên kết hồ sơ giảng viên.";
                return RedirectToAction("Index", "GiangVien");
            }

            // Giảng viên bắt buộc phải gắn với một lớp học phần do mình phụ trách
            if (lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
            {
                var canAccess = await _securityService.CanAccessLopHocPhanAsync(User, lopHocPhanId.Value);
                if (!canAccess)
                {
                    _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_GET", $"LopHocPhanId={lopHocPhanId.Value}", "Giảng viên cố tình truy cập form nhập điểm lớp học phần của giảng viên khác.");
                    TempData["Error"] = "Bạn không có quyền nhập điểm cho lớp học phần của giảng viên khác!";
                    return RedirectToAction(nameof(Index));
                }

                if (sinhVienId.HasValue && sinhVienId.Value > 0)
                {
                    var canGrade = await _securityService.CanGradeStudentAsync(User, lopHocPhanId.Value, sinhVienId.Value);
                    if (!canGrade)
                    {
                        _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_GET", $"LopHocPhanId={lopHocPhanId.Value}&SinhVienId={sinhVienId.Value}", "Giảng viên cố tình nhập điểm cho sinh viên không đăng ký lớp học phần này.");
                        TempData["Error"] = "Sinh viên này không đăng ký lớp học phần của bạn!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
        }

        await LoadDropdownsAsync(isGiangVienOnly, giangVien?.Id, lopHocPhanId, sinhVienId, monHocId);

        var model = new DiemFormViewModel
        {
            LopHocPhanId = lopHocPhanId,
            SinhVienId = sinhVienId ?? 0,
            MonHocId = monHocId ?? 0,
            LoaiHoc = "HOC_LAN_DAU",
            LanHoc = 1,
            LanThi = 1,
            SuDungTinhTichLuy = true
        };

        if (lopHocPhanId.HasValue && lopHocPhanId.Value > 0)
        {
            var lhp = await _db.LopHocPhans.Include(l => l.HocKy).FirstOrDefaultAsync(l => l.Id == lopHocPhanId.Value);
            if (lhp != null)
            {
                model.MonHocId = lhp.MonHocId;
                model.TenLopHocPhan = lhp.TenLopHocPhan;
                model.HocKyId = lhp.HocKyId;
            }
        }

        // Tự động phân tích lịch sử các lần học trước của sinh viên cho môn học này
        if (model.SinhVienId > 0 && model.MonHocId > 0)
        {
            var lichSu = await _db.Diems
                .Include(d => d.LopHocPhan)
                .Include(d => d.HocKy)
                .Where(d => d.SinhVienId == model.SinhVienId && d.MonHocId == model.MonHocId)
                .OrderBy(d => d.LanHoc)
                .ThenBy(d => d.LanThi)
                .ToListAsync();

            model.LichSuDiem = lichSu;

            // Nếu sinh viên đã có điểm trong Lớp học phần này -> Load lại điểm để sửa hoặc thi lại
            Diem? diemHienTai = null;
            if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0)
            {
                diemHienTai = lichSu.FirstOrDefault(d => d.LopHocPhanId == model.LopHocPhanId.Value);
            }

            if (diemHienTai != null)
            {
                model.Id = diemHienTai.Id;
                model.DiemChuyenCan = diemHienTai.DiemChuyenCan;
                model.DiemGiuaKy = diemHienTai.DiemGiuaKy;
                model.DiemCuoiKy = diemHienTai.DiemCuoiKy;
                model.DiemCuoiKyLan2 = diemHienTai.DiemCuoiKyLan2;
                model.LoaiHoc = diemHienTai.LoaiHoc;
                model.LanHoc = diemHienTai.LanHoc;
                model.LanThi = diemHienTai.LanThi;
                model.GhiChu = diemHienTai.GhiChu;
                model.SuDungTinhTichLuy = diemHienTai.SuDungTinhTichLuy;
                model.HocKyId = diemHienTai.HocKyId;
            }
            else
            {
                // Chưa có điểm ở lớp HP này, tính toán lần học tiếp theo
                var diemCuoi = lichSu.LastOrDefault();
                if (diemCuoi != null)
                {
                    int lanHocCuoi = lichSu.Max(d => d.LanHoc);
                    model.LanHoc = lanHocCuoi + 1;

                    if (!diemCuoi.IsDat)
                    {
                        model.LoaiHoc = "HOC_LAI";
                        model.GhiChu = $"Học lại môn (lần {lanHocCuoi} chưa đạt, TK: {diemCuoi.DiemTongKet})";
                    }
                    else
                    {
                        model.LoaiHoc = "HOC_CAI_THIEN";
                        model.GhiChu = $"Học cải thiện điểm (lần {lanHocCuoi} đã đạt {diemCuoi.DiemTongKet} điểm)";
                    }
                }
            }
        }

        return View(model);
    }

    // API lấy nhanh thông tin điểm theo Lớp HP & Sinh viên (AJAX không reload trang)
    [HttpGet]
    public async Task<IActionResult> GetThongTinDiemJson(int? lopHocPhanId, int? sinhVienId)
    {
        var lhp = await _db.LopHocPhans.FindAsync(lopHocPhanId ?? 0);
        if (lhp == null) return Json(new { success = false });

        var d = sinhVienId > 0 ? await _db.Diems.FirstOrDefaultAsync(x => x.LopHocPhanId == lopHocPhanId && x.SinhVienId == sinhVienId) : null;
        return Json(new
        {
            success = true,
            hasData = d != null,
            id = d?.Id ?? 0,
            monHocId = lhp.MonHocId,
            hocKyId = lhp.HocKyId,
            cc = d?.DiemChuyenCan,
            gk = d?.DiemGiuaKy,
            ck = d?.DiemCuoiKy,
            ck2 = d?.DiemCuoiKyLan2,
            lanHoc = d?.LanHoc,
            lanThi = d?.LanThi,
            loaiHoc = d?.LoaiHoc,
            ghiChu = d?.GhiChu
        });
    }

    // 3. Nhập điểm môn học (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapDiem(DiemFormViewModel model)
    {
        var giangVien = await _securityService.GetCurrentGiangVienAsync(User);
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        // 6 Lớp Kiểm Tra Bảo Mật Phía Server
        if (isGiangVienOnly)
        {
            if (giangVien == null)
            {
                TempData["Error"] = "Tài khoản của bạn chưa được liên kết hồ sơ giảng viên.";
                return RedirectToAction("Index", "GiangVien");
            }

            if (!model.LopHocPhanId.HasValue || model.LopHocPhanId.Value <= 0)
            {
                _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_POST", "LopHocPhanId=null", "Giảng viên cố tình gửi POST nhập điểm không gắn với Lớp học phần.");
                TempData["Error"] = "Vui lòng chọn Lớp học phần bạn được phân công giảng dạy.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra giảng viên có dạy lớp học phần này không
            var canAccess = await _securityService.CanAccessLopHocPhanAsync(User, model.LopHocPhanId.Value);
            if (!canAccess)
            {
                _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_POST", $"LopHocPhanId={model.LopHocPhanId.Value}", "Giảng viên cố tình ghi điểm vào lớp học phần không phụ trách.");
                TempData["Error"] = "Bạn không có quyền nhập điểm cho lớp học phần này!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra sinh viên có thực sự đăng ký lớp học phần này không
            var canGrade = await _securityService.CanGradeStudentAsync(User, model.LopHocPhanId.Value, model.SinhVienId);
            if (!canGrade)
            {
                _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_POST", $"LopHocPhanId={model.LopHocPhanId.Value}&SinhVienId={model.SinhVienId}", "Giảng viên cố tình ghi điểm cho sinh viên không đăng ký lớp.");
                TempData["Error"] = "Sinh viên này không đăng ký lớp học phần của bạn!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu sửa điểm (model.Id > 0): Kiểm tra bản ghi điểm có thuộc lớp học phần do mình phụ trách không
            if (model.Id > 0)
            {
                var canModify = await _securityService.CanModifyDiemAsync(User, model.Id);
                if (!canModify)
                {
                    _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_POST", $"DiemId={model.Id}", "Giảng viên cố tình sửa bản ghi điểm không thuộc quyền quản lý.");
                    TempData["Error"] = "Bạn không có quyền sửa bản ghi điểm này!";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0 && model.MonHocId <= 0)
        {
            var lhp = await _db.LopHocPhans.FindAsync(model.LopHocPhanId.Value);
            if (lhp != null)
            {
                model.MonHocId = lhp.MonHocId;
                if (!model.HocKyId.HasValue) model.HocKyId = lhp.HocKyId;
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(isGiangVienOnly, giangVien?.Id, model.LopHocPhanId, model.SinhVienId, model.MonHocId, model.HocKyId);
            return View(model);
        }

        // Tìm bản ghi điểm đã có trong Lớp học phần hiện tại (hoặc theo ID)
        Diem? diem = null;
        if (model.Id > 0)
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d => d.Id == model.Id);
        }
        else if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0)
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d =>
                d.SinhVienId == model.SinhVienId && d.LopHocPhanId == model.LopHocPhanId.Value);
        }
        else
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d =>
                d.SinhVienId == model.SinhVienId && d.MonHocId == model.MonHocId && d.LanHoc == model.LanHoc);
        }

        var currentUserName = User.Identity?.Name ?? "Hệ thống";

        if (diem != null)
        {
            diem.DiemChuyenCan = model.DiemChuyenCan!.Value;
            diem.DiemGiuaKy = model.DiemGiuaKy!.Value;
            diem.DiemCuoiKy = model.DiemCuoiKy!.Value;
            diem.DiemCuoiKyLan2 = model.DiemCuoiKyLan2;
            diem.LoaiHoc = model.LoaiHoc;
            diem.LanHoc = model.LanHoc;
            diem.LanThi = model.LanThi;
            diem.GhiChu = model.GhiChu;
            diem.HocKyId = model.HocKyId;
            diem.NgayCapNhat = DateTime.UtcNow;
            diem.NguoiCapNhat = currentUserName;
            if (model.LopHocPhanId.HasValue) diem.LopHocPhanId = model.LopHocPhanId.Value;

            diem.TrangThai = diem.IsDat ? "DAT" : "KHONG_DAT";
            TempData["Msg"] = "Đã cập nhật điểm học phần thành công!";
        }
        else
        {
            diem = new Diem
            {
                SinhVienId = model.SinhVienId,
                MonHocId = model.MonHocId,
                LopHocPhanId = model.LopHocPhanId,
                HocKyId = model.HocKyId,
                LoaiHoc = model.LoaiHoc,
                LanHoc = model.LanHoc,
                LanThi = model.LanThi,
                DiemChuyenCan = model.DiemChuyenCan!.Value,
                DiemGiuaKy = model.DiemGiuaKy!.Value,
                DiemCuoiKy = model.DiemCuoiKy!.Value,
                DiemCuoiKyLan2 = model.DiemCuoiKyLan2,
                GhiChu = model.GhiChu,
                NgayCapNhat = DateTime.UtcNow,
                NguoiCapNhat = currentUserName
            };
            diem.TrangThai = diem.IsDat ? "DAT" : "KHONG_DAT";
            _db.Diems.Add(diem);
            TempData["Msg"] = "Đã ghi nhận điểm học phần mới thành công!";
        }

        await _db.SaveChangesAsync();

        // Cập nhật cờ SuDungTinhTichLuy
        await CapNhatCacDiemTichLuyCuaSinhVienAsync(diem.SinhVienId, diem.MonHocId);

        return RedirectToAction(nameof(Index), new { lopHocPhanId = model.LopHocPhanId });
    }

    private async Task CapNhatCacDiemTichLuyCuaSinhVienAsync(int sinhVienId, int monHocId)
    {
        var allDiems = await _db.Diems
            .Where(d => d.SinhVienId == sinhVienId && d.MonHocId == monHocId)
            .OrderBy(d => d.LanHoc)
            .ThenBy(d => d.LanThi)
            .ToListAsync();

        if (allDiems.Count == 0) return;

        var banGhiTichLuy = allDiems.LastOrDefault(d => d.IsDat);
        if (banGhiTichLuy == null)
        {
            banGhiTichLuy = allDiems.LastOrDefault();
        }

        foreach (var item in allDiems)
        {
            item.SuDungTinhTichLuy = (banGhiTichLuy != null && item.Id == banGhiTichLuy.Id);
            item.TrangThai = item.IsDat ? "DAT" : "KHONG_DAT";
        }

        await _db.SaveChangesAsync();
    }

    private async Task LoadDropdownsAsync(
        bool isGiangVienOnly,
        int? giangVienId,
        int? selectedLhp = null,
        int? selectedSv = null,
        int? selectedMon = null,
        int? selectedHocKy = null)
    {
        var queryLhp = _db.LopHocPhans
            .Include(l => l.MonHoc)
            .Include(l => l.HocKy)
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
                .Where(dk => dk.LopHocPhanId == selectedLhp.Value && dk.TrangThai == EnrollmentStatus.Enrolled)
                .OrderBy(dk => dk.SinhVien.HoTen)
                .Select(dk => new
                {
                    dk.SinhVien.Id,
                    Text = $"{dk.SinhVien.MaSinhVien} - {dk.SinhVien.HoTen} (Lớp: {dk.SinhVien.Lop!.TenLop})"
                })
                .ToListAsync();

            ViewBag.DanhSachSinhVien = new SelectList(svTrongLhp, "Id", "Text", selectedSv);
        }
        else if (isGiangVienOnly && giangVienId.HasValue)
        {
            // Nếu là giảng viên mà chưa chọn lớp HP: Chỉ hiện các sinh viên trong các lớp HP của giảng viên này
            var svCuaGv = await _db.DangKyHocPhans
                .Include(dk => dk.SinhVien)
                    .ThenInclude(s => s.Lop)
                .Where(dk => dk.LopHocPhan.GiangVienId == giangVienId.Value && dk.TrangThai == EnrollmentStatus.Enrolled)
                .Select(dk => dk.SinhVien)
                .Distinct()
                .OrderBy(s => s.HoTen)
                .Select(s => new
                {
                    s.Id,
                    Text = $"{s.MaSinhVien} - {s.HoTen} (Lớp: {s.Lop!.TenLop})"
                })
                .ToListAsync();

            ViewBag.DanhSachSinhVien = new SelectList(svCuaGv, "Id", "Text", selectedSv);
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

        ViewBag.DanhSachHocKy = new SelectList(
            await _db.HocKies.OrderByDescending(h => h.NgayBatDau).ToListAsync(),
            "HocKyId", "TenHocKy", selectedHocKy);
    }
}
