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

    // 2. Nhập điểm môn học dạng Excel theo Khoa -> Lớp -> Môn học (GET)
    [HttpGet]
    public async Task<IActionResult> NhapDiem(int? khoaId, int? lopId, int? monHocId, int? lopHocPhanId, int? sinhVienId)
    {
        var giangVien = await _securityService.GetCurrentGiangVienAsync(User);
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        if (isGiangVienOnly && giangVien == null)
        {
            TempData["Error"] = "Tài khoản của bạn chưa được liên kết hồ sơ giảng viên.";
            return RedirectToAction("Index", "GiangVien");
        }

        // Hỗ trợ trường hợp truyền link cũ có sinhVienId hoặc lopHocPhanId
        if (sinhVienId.HasValue && !lopId.HasValue)
        {
            var sv = await _db.SinhViens.FindAsync(sinhVienId.Value);
            if (sv != null) lopId = sv.LopId;
        }
        if (lopHocPhanId.HasValue && !monHocId.HasValue)
        {
            var lhp = await _db.LopHocPhans.FindAsync(lopHocPhanId.Value);
            if (lhp != null) monHocId = lhp.MonHocId;
        }

        // 1. Danh sách Khoa (Chỉ phần giảng viên có quyền)
        List<DonVi> dsKhoa;
        if (isGiangVienOnly && giangVien != null)
        {
            dsKhoa = await _db.DonVis
                .Where(d => d.DonViId == giangVien.DonViId || d.GiangViens.Any(g => g.Id == giangVien.Id))
                .ToListAsync();

            if (!khoaId.HasValue && giangVien.DonViId.HasValue)
            {
                khoaId = giangVien.DonViId.Value;
            }
        }
        else
        {
            dsKhoa = await _db.DonVis
                .Where(d => d.LoaiDonVi == "KHOA" || d.DonViChaId == 1 || d.DonViChaId == null)
                .OrderBy(d => d.TenDonVi)
                .ToListAsync();

            if (!khoaId.HasValue && dsKhoa.Any())
            {
                khoaId = dsKhoa.First().DonViId;
            }
        }

        // 2. Danh sách Lớp theo Khoa đã chọn
        List<Lop> dsLop = new();
        if (khoaId.HasValue)
        {
            dsLop = await _db.Lops
                .Include(l => l.Nganh)
                .Where(l => l.Nganh != null && (l.Nganh.DonViId == khoaId.Value || l.Nganh.DonVi!.DonViChaId == khoaId.Value))
                .OrderBy(l => l.TenLop)
                .ToListAsync();
        }
        else
        {
            dsLop = await _db.Lops.OrderBy(l => l.TenLop).ToListAsync();
        }

        // 3. Danh sách Môn học (Chỉ những môn giảng viên có quyền)
        List<MonHoc> dsMonHoc = new();
        List<int> gvMonHocIds = new();
        if (isGiangVienOnly && giangVien != null)
        {
            gvMonHocIds = await _db.LopHocPhans
                .Where(l => l.GiangVienId == giangVien.Id)
                .Select(l => l.MonHocId)
                .Distinct()
                .ToListAsync();

            if (gvMonHocIds.Any())
            {
                var queryMon = _db.MonHocs.Where(m => gvMonHocIds.Contains(m.Id));
                if (khoaId.HasValue) queryMon = queryMon.Where(m => m.DonViId == khoaId.Value);
                dsMonHoc = await queryMon.OrderBy(m => m.TenMon).ToListAsync();
            }
            else
            {
                dsMonHoc = await _db.MonHocs
                    .Where(m => m.DonViId == khoaId)
                    .OrderBy(m => m.TenMon)
                    .ToListAsync();
            }
        }
        else
        {
            var queryMon = _db.MonHocs.AsQueryable();
            if (khoaId.HasValue) queryMon = queryMon.Where(m => m.DonViId == khoaId.Value);
            dsMonHoc = await queryMon.OrderBy(m => m.TenMon).ToListAsync();
        }

        ViewBag.DanhSachKhoa = new SelectList(dsKhoa, "DonViId", "TenDonVi", khoaId);
        ViewBag.DanhSachLop = new SelectList(dsLop, "Id", "TenLop", lopId);
        ViewBag.DanhSachMonHoc = new SelectList(
            dsMonHoc.Select(m => new { m.Id, TenHienThi = $"{m.MaMon} - {m.TenMon} ({m.SoTinChi} TC)" }),
            "Id", "TenHienThi", monHocId);

        ViewBag.KhoaId = khoaId;
        ViewBag.LopId = lopId;
        ViewBag.MonHocId = monHocId;

        // Khởi tạo ViewModel bảng điểm Excel
        var model = new NhapDiemExcelViewModel
        {
            KhoaId = khoaId,
            LopId = lopId,
            MonHocId = monHocId
        };

        if (khoaId.HasValue)
        {
            var k = await _db.DonVis.FindAsync(khoaId.Value);
            model.TenKhoa = k?.TenDonVi;
        }

        if (lopId.HasValue)
        {
            var l = await _db.Lops.FindAsync(lopId.Value);
            model.TenLop = l?.TenLop;
        }

        if (monHocId.HasValue)
        {
            var m = await _db.MonHocs.FindAsync(monHocId.Value);
            if (m != null)
            {
                model.TenMonHoc = m.TenMon;
                model.MaMon = m.MaMon;
                model.SoTinChi = m.SoTinChi;
            }
        }

        // Khi đã chọn Lớp và Môn học: Nạp toàn bộ sinh viên trong lớp và điểm số hiện có
        if (lopId.HasValue && monHocId.HasValue)
        {
            // Kiểm tra bảo mật nếu là giảng viên
            if (isGiangVienOnly && giangVien != null && gvMonHocIds.Any() && !gvMonHocIds.Contains(monHocId.Value))
            {
                _securityService.LogSecurityViolation(User, "BangDiem.NhapDiem_GET", $"MonHocId={monHocId.Value}", "Giảng viên cố tình chọn môn học không được phân công.");
                TempData["Error"] = "Bạn không có quyền nhập điểm cho môn học này!";
                return RedirectToAction(nameof(NhapDiem), new { khoaId });
            }

            // Tìm LopHocPhan tương ứng
            var lhpQuery = _db.LopHocPhans.Include(l => l.HocKy).Where(l => l.MonHocId == monHocId.Value);
            if (isGiangVienOnly && giangVien != null)
            {
                lhpQuery = lhpQuery.Where(l => l.GiangVienId == giangVien.Id);
            }
            var lhp = await lhpQuery.FirstOrDefaultAsync();
            if (lhp != null)
            {
                model.LopHocPhanId = lhp.Id;
                model.HocKyId = lhp.HocKyId;
                model.TenLopHocPhan = lhp.TenLopHocPhan;
            }

            var sinhViens = await _db.SinhViens
                .Include(s => s.Lop)
                .Where(s => s.LopId == lopId.Value)
                .OrderBy(s => s.MaSinhVien)
                .ToListAsync();

            var svIds = sinhViens.Select(s => s.Id).ToList();
            var existingDiems = await _db.Diems
                .Where(d => svIds.Contains(d.SinhVienId) && d.MonHocId == monHocId.Value)
                .ToListAsync();

            foreach (var sv in sinhViens)
            {
                var d = existingDiems.FirstOrDefault(x => x.SinhVienId == sv.Id);
                var row = new BangDiemExcelRowModel
                {
                    SinhVienId = sv.Id,
                    MaSinhVien = sv.MaSinhVien,
                    HoTen = sv.HoTen,
                    NgaySinhStr = sv.NgaySinh?.ToString("dd/MM/yyyy"),
                    TenLop = sv.Lop?.TenLop,
                    DiemId = d?.Id,
                    LopHocPhanId = d?.LopHocPhanId ?? lhp?.Id,
                    DiemChuyenCan = d?.DiemChuyenCan,
                    DiemGiuaKy = d?.DiemGiuaKy,
                    DiemCuoiKy = d?.DiemCuoiKy,
                    DiemCuoiKyLan2 = d?.DiemCuoiKyLan2,
                    DiemTongKet = (d != null && d.DiemChuyenCan > 0 && d.DiemGiuaKy > 0 && d.DiemCuoiKy > 0) ? d.DiemTongKet : null,
                    DiemChu = (d != null && d.DiemChuyenCan > 0 && d.DiemGiuaKy > 0 && d.DiemCuoiKy > 0) ? d.DiemChu : null,
                    DiemHe4 = (d != null && d.DiemChuyenCan > 0 && d.DiemGiuaKy > 0 && d.DiemCuoiKy > 0) ? d.DiemHe4 : null,
                    TrangThai = d?.TrangThai,
                    GhiChu = d?.GhiChu
                };
                model.DanhSachSinhVien.Add(row);
            }
        }

        return View(model);
    }

    // AJAX API: Lấy danh sách Lớp theo Khoa
    [HttpGet]
    public async Task<IActionResult> GetLopsByKhoa(int khoaId)
    {
        var lops = await _db.Lops
            .Include(l => l.Nganh)
            .Where(l => l.Nganh != null && (l.Nganh.DonViId == khoaId || l.Nganh.DonVi!.DonViChaId == khoaId))
            .OrderBy(l => l.TenLop)
            .Select(l => new { id = l.Id, tenLop = l.TenLop })
            .ToListAsync();
        return Json(lops);
    }

    // AJAX API: Lấy danh sách Môn học theo Khoa (lọc theo quyền giảng viên)
    [HttpGet]
    public async Task<IActionResult> GetMonHocsByKhoa(int? khoaId, int? lopId)
    {
        var giangVien = await _securityService.GetCurrentGiangVienAsync(User);
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        if (isGiangVienOnly && giangVien != null)
        {
            var gvMonHocIds = await _db.LopHocPhans
                .Where(l => l.GiangVienId == giangVien.Id)
                .Select(l => l.MonHocId)
                .Distinct()
                .ToListAsync();

            var query = _db.MonHocs.AsQueryable();
            if (gvMonHocIds.Any())
            {
                query = query.Where(m => gvMonHocIds.Contains(m.Id));
            }
            if (khoaId.HasValue)
            {
                query = query.Where(m => m.DonViId == khoaId.Value);
            }

            var mons = await query.OrderBy(m => m.TenMon)
                .Select(m => new { id = m.Id, maMon = m.MaMon, tenMon = m.TenMon, soTinChi = m.SoTinChi, tenHienThi = $"{m.MaMon} - {m.TenMon} ({m.SoTinChi} TC)" })
                .ToListAsync();
            return Json(mons);
        }
        else
        {
            var query = _db.MonHocs.AsQueryable();
            if (khoaId.HasValue) query = query.Where(m => m.DonViId == khoaId.Value);

            var mons = await query.OrderBy(m => m.TenMon)
                .Select(m => new { id = m.Id, maMon = m.MaMon, tenMon = m.TenMon, soTinChi = m.SoTinChi, tenHienThi = $"{m.MaMon} - {m.TenMon} ({m.SoTinChi} TC)" })
                .ToListAsync();
            return Json(mons);
        }
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

    // 3. Nhập điểm môn học dạng Excel theo Lớp (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapDiem(NhapDiemExcelViewModel model)
    {
        if (!model.MonHocId.HasValue || !model.LopId.HasValue)
        {
            TempData["Error"] = "Vui lòng chọn đầy đủ Lớp và Môn học để lưu điểm.";
            return RedirectToAction(nameof(NhapDiem), new { khoaId = model.KhoaId, lopId = model.LopId, monHocId = model.MonHocId });
        }

        var giangVien = await _securityService.GetCurrentGiangVienAsync(User);
        var isGiangVienOnly = User.IsInRole("GiangVien") && !User.IsInRole("Admin") && !User.IsInRole("PGV");

        if (isGiangVienOnly)
        {
            if (giangVien == null)
            {
                TempData["Error"] = "Tài khoản của bạn chưa được liên kết hồ sơ giảng viên.";
                return RedirectToAction("Index", "GiangVien");
            }

            var gvMonHocIds = await _db.LopHocPhans
                .Where(l => l.GiangVienId == giangVien.Id)
                .Select(l => l.MonHocId)
                .Distinct()
                .ToListAsync();

            if (gvMonHocIds.Any() && !gvMonHocIds.Contains(model.MonHocId.Value))
            {
                _securityService.LogSecurityViolation(User, "BangDiem.NhapDiemExcel_POST", $"MonHocId={model.MonHocId.Value}", "Giảng viên cố tình lưu điểm môn học không phụ trách.");
                TempData["Error"] = "Bạn không có quyền nhập điểm cho môn học này!";
                return RedirectToAction(nameof(NhapDiem), new { khoaId = model.KhoaId });
            }
        }

        // Tìm LopHocPhan tương ứng
        var lhpQuery = _db.LopHocPhans.Where(l => l.MonHocId == model.MonHocId.Value);
        if (isGiangVienOnly && giangVien != null)
        {
            lhpQuery = lhpQuery.Where(l => l.GiangVienId == giangVien.Id);
        }
        var lhp = await lhpQuery.FirstOrDefaultAsync();

        var currentUserName = User.Identity?.Name ?? "Giảng viên";
        int savedCount = 0;
        var svIdsToUpdate = new List<int>();

        foreach (var row in model.DanhSachSinhVien)
        {
            bool hasAnyScore = row.DiemChuyenCan.HasValue || row.DiemGiuaKy.HasValue || row.DiemCuoiKy.HasValue;
            if (!hasAnyScore) continue;

            double cc = Math.Clamp(row.DiemChuyenCan ?? 0, 0, 10);
            double gk = Math.Clamp(row.DiemGiuaKy ?? 0, 0, 10);
            double ck = Math.Clamp(row.DiemCuoiKy ?? 0, 0, 10);

            Diem? diem = null;
            if (row.DiemId.HasValue && row.DiemId.Value > 0)
            {
                diem = await _db.Diems.FirstOrDefaultAsync(d => d.Id == row.DiemId.Value);
            }
            else
            {
                diem = await _db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == row.SinhVienId && d.MonHocId == model.MonHocId.Value);
            }

            if (diem != null)
            {
                diem.DiemChuyenCan = cc;
                diem.DiemGiuaKy = gk;
                diem.DiemCuoiKy = ck;
                if (row.DiemCuoiKyLan2.HasValue) diem.DiemCuoiKyLan2 = Math.Clamp(row.DiemCuoiKyLan2.Value, 0, 10);
                diem.GhiChu = row.GhiChu;
                diem.NgayCapNhat = DateTime.UtcNow;
                diem.NguoiCapNhat = currentUserName;
                if (lhp != null)
                {
                    diem.LopHocPhanId = lhp.Id;
                    diem.HocKyId = lhp.HocKyId;
                }
                diem.TrangThai = diem.IsDat ? "DAT" : "KHONG_DAT";
            }
            else
            {
                diem = new Diem
                {
                    SinhVienId = row.SinhVienId,
                    MonHocId = model.MonHocId.Value,
                    LopHocPhanId = lhp?.Id,
                    HocKyId = lhp?.HocKyId,
                    LoaiHoc = "HOC_LAN_DAU",
                    LanHoc = 1,
                    LanThi = 1,
                    DiemChuyenCan = cc,
                    DiemGiuaKy = gk,
                    DiemCuoiKy = ck,
                    DiemCuoiKyLan2 = row.DiemCuoiKyLan2.HasValue ? Math.Clamp(row.DiemCuoiKyLan2.Value, 0, 10) : null,
                    GhiChu = row.GhiChu,
                    NgayCapNhat = DateTime.UtcNow,
                    NguoiCapNhat = currentUserName,
                    SuDungTinhTichLuy = true
                };
                diem.TrangThai = diem.IsDat ? "DAT" : "KHONG_DAT";
                _db.Diems.Add(diem);
            }

            if (lhp != null)
            {
                var enrolled = await _db.DangKyHocPhans.AnyAsync(dk => dk.LopHocPhanId == lhp.Id && dk.SinhVienId == row.SinhVienId);
                if (!enrolled)
                {
                    _db.DangKyHocPhans.Add(new DangKyHocPhan
                    {
                        LopHocPhanId = lhp.Id,
                        SinhVienId = row.SinhVienId,
                        NgayDangKy = DateTime.UtcNow,
                        TrangThai = "ENROLLED",
                        LanHoc = 1,
                        LoaiDangKy = "HOC_LAN_DAU"
                    });
                }
            }

            svIdsToUpdate.Add(row.SinhVienId);
            savedCount++;
        }

        await _db.SaveChangesAsync();

        foreach (var svId in svIdsToUpdate.Distinct())
        {
            await CapNhatCacDiemTichLuyCuaSinhVienAsync(svId, model.MonHocId.Value);
        }

        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        if (isAjax)
        {
            return Json(new { success = true, count = savedCount, message = $"Đã lưu bảng điểm thành công cho {savedCount} sinh viên!" });
        }

        TempData["Msg"] = $"Đã lưu bảng điểm thành công cho {savedCount} sinh viên môn {model.TenMonHoc ?? ""} - Lớp {model.TenLop ?? ""}!";
        return RedirectToAction(nameof(NhapDiem), new { khoaId = model.KhoaId, lopId = model.LopId, monHocId = model.MonHocId });
    }

    // 4. Nhập điểm đơn lẻ cho 1 sinh viên (Hỗ trợ cấu hình lần học/lần thi/lịch sử học lại)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapDiemDon(DiemFormViewModel model)
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

            if (model.LopHocPhanId.HasValue && model.LopHocPhanId.Value > 0)
            {
                var canAccess = await _securityService.CanAccessLopHocPhanAsync(User, model.LopHocPhanId.Value);
                if (!canAccess)
                {
                    _securityService.LogSecurityViolation(User, "BangDiem.NhapDiemDon_POST", $"LopHocPhanId={model.LopHocPhanId.Value}", "Giảng viên cố tình ghi điểm vào lớp học phần không phụ trách.");
                    TempData["Error"] = "Bạn không có quyền nhập điểm cho lớp học phần này!";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu nhập không hợp lệ, vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(NhapDiem));
        }

        Diem? diem = null;
        if (model.Id > 0)
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d => d.Id == model.Id);
        }
        else
        {
            diem = await _db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == model.SinhVienId && d.MonHocId == model.MonHocId && d.LanHoc == model.LanHoc);
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
        else if (isGiangVienOnly && giangVienId.HasValue)
        {
            // Nếu là giảng viên mà chưa chọn lớp HP: Chỉ hiện các sinh viên trong các lớp HP của giảng viên này
            var svCuaGv = await _db.DangKyHocPhans
                .Include(dk => dk.SinhVien)
                    .ThenInclude(s => s.Lop)
                .Where(dk => dk.LopHocPhan.GiangVienId == giangVienId.Value && dk.TrangThai == "ENROLLED")
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
