using System.Linq;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ClosedXML.Excel;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class QuanLySinhVienController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;

    public QuanLySinhVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService)
    {
        _db = db;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? lopId, string? trangThai, int page = 1)
    {
        const int pageSize = 10;
        page = Math.Max(page, 1);

        var query = _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
                .ThenInclude(d => d.MonHoc)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(s => s.MaSinhVien.Contains(keyword) || s.HoTen.Contains(keyword));
        }

        if (lopId.HasValue)
        {
            query = query.Where(s => s.LopId == lopId.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            var trangThaiTrim = trangThai.Trim();
            query = query.Where(s => s.TrangThai.Trim() == trangThaiTrim);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page > Math.Max(totalPages, 1))
        {
            page = Math.Max(totalPages, 1);
        }

        var items = await query
            .OrderBy(s => s.MaSinhVien)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // LÀM VIỆC VỚI SESSION: Lưu lại tiêu chí lọc gần nhất của người dùng
        if (!string.IsNullOrWhiteSpace(tuKhoa) || lopId.HasValue || !string.IsNullOrWhiteSpace(trangThai))
        {
            HttpContext.Session.SetString("QLSV_LastTuKhoa", tuKhoa ?? "");
            HttpContext.Session.SetString("QLSV_LastLopId", lopId?.ToString() ?? "");
            HttpContext.Session.SetString("QLSV_LastTrangThai", trangThai ?? "");
            HttpContext.Session.SetString("QLSV_LastSearchTime", DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"));
        }

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LopId = lopId;
        ViewBag.TrangThai = trangThai;
        ViewBag.LastSearchTime = HttpContext.Session.GetString("QLSV_LastSearchTime");
        ViewBag.DanhSachLop = new SelectList(
            await _db.Lops.OrderBy(l => l.TenLop).ToListAsync(),
            "Id",
            "TenLop",
            lopId);

        return View(new PagedResult<SinhVien>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var sinhVien = await _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
                .ThenInclude(d => d.MonHoc)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sinhVien == null)
        {
            return NotFound();
        }

        return View(sinhVien);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLopAsync();
        return View("Form", new SinhVienFormViewModel { TrangThai = "Đang học" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SinhVienFormViewModel model)
    {
        Normalize(model);

        if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == model.MaSinhVien))
        {
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadLopAsync(model.LopId);
            return View("Form", model);
        }

        string? avatarPath = null;
        if (model.FileAnh != null && model.FileAnh.Length > 0)
        {
            avatarPath = await _fileUploadService.UploadAvatarAsync(model.FileAnh);
        }

        var sinhVien = new SinhVien
        {
            MaSinhVien = model.MaSinhVien,
            HoTen = model.HoTen,
            NgaySinh = model.NgaySinh,
            GioiTinh = model.GioiTinh,
            Email = model.Email,
            SoDienThoai = model.SoDienThoai,
            DiaChi = model.DiaChi,
            LopId = model.LopId,
            NienKhoa = model.NienKhoa,
            TrangThai = model.TrangThai,
            HinhAnh = avatarPath
        };

        _db.SinhViens.Add(sinhVien);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã thêm sinh viên";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? tuKhoa, int? lopId, string? trangThai)
    {
        var query = _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(s => s.MaSinhVien.Contains(keyword) || s.HoTen.Contains(keyword));
        }
        if (lopId.HasValue)
            query = query.Where(s => s.LopId == lopId.Value);
        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(s => s.TrangThai == trangThai);

        var items = await query.OrderBy(s => s.MaSinhVien).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DanhSachSinhVien");
        ws.Cell(1, 1).Value = "Mã SV";
        ws.Cell(1, 2).Value = "Họ tên";
        ws.Cell(1, 3).Value = "Ngày sinh";
        ws.Cell(1, 4).Value = "Giới tính";
        ws.Cell(1, 5).Value = "Email";
        ws.Cell(1, 6).Value = "Số điện thoại";
        ws.Cell(1, 7).Value = "Địa chỉ";
        ws.Cell(1, 8).Value = "Lớp";
        ws.Cell(1, 9).Value = "Ngành";
        ws.Cell(1, 10).Value = "Niên khóa";
        ws.Cell(1, 11).Value = "Trạng thái";

        int r = 2;
        foreach (var s in items)
        {
            ws.Cell(r, 1).Value = s.MaSinhVien;
            ws.Cell(r, 2).Value = s.HoTen;
            ws.Cell(r, 3).Value = s.NgaySinh?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(r, 4).Value = s.GioiTinh ?? "";
            ws.Cell(r, 5).Value = s.Email ?? "";
            ws.Cell(r, 6).Value = s.SoDienThoai ?? "";
            ws.Cell(r, 7).Value = s.DiaChi ?? "";
            ws.Cell(r, 8).Value = s.Lop?.TenLop ?? "";
            ws.Cell(r, 9).Value = s.Lop?.Nganh?.TenNganh ?? "";
            ws.Cell(r, 10).Value = s.NienKhoa ?? "";
            ws.Cell(r, 11).Value = s.TrangThai ?? "";
            r++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet]
    public IActionResult ExportSample()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("MauSinhVien");
        ws.Cell(1, 1).Value = "Mã SV";
        ws.Cell(1, 2).Value = "Họ tên";
        ws.Cell(1, 3).Value = "Ngày sinh";
        ws.Cell(1, 4).Value = "Giới tính";
        ws.Cell(1, 5).Value = "Email";
        ws.Cell(1, 6).Value = "Số điện thoại";
        ws.Cell(1, 7).Value = "Địa chỉ";
        ws.Cell(1, 8).Value = "Tên lớp";
        ws.Cell(1, 9).Value = "Niên khóa";
        ws.Cell(1, 10).Value = "Trạng thái";

        ws.Cell(2, 1).Value = "SV20240001";
        ws.Cell(2, 2).Value = "Nguyễn Văn A";
        ws.Cell(2, 3).Value = "01/01/2005";
        ws.Cell(2, 4).Value = "Nam";
        ws.Cell(2, 5).Value = "nguyenvana@utc.edu.vn";
        ws.Cell(2, 6).Value = "0912345678";
        ws.Cell(2, 7).Value = "Hà Nội";
        ws.Cell(2, 8).Value = "CT123";
        ws.Cell(2, 9).Value = "K2024";
        ws.Cell(2, 10).Value = "Đang học";

        ws.Cell(3, 1).Value = "SV20240002";
        ws.Cell(3, 2).Value = "Trần Thị B";
        ws.Cell(3, 3).Value = "15/03/2005";
        ws.Cell(3, 4).Value = "Nữ";
        ws.Cell(3, 5).Value = "tranthib@utc.edu.vn";
        ws.Cell(3, 6).Value = "0987654321";
        ws.Cell(3, 7).Value = "TP Hồ Chí Minh";
        ws.Cell(3, 8).Value = "CT124";
        ws.Cell(3, 9).Value = "K2024";
        ws.Cell(3, 10).Value = "Đang học";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauSinhVien.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file Excel.";
            return RedirectToAction(nameof(Index));
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Chỉ接受 file Excel (.xlsx, .xls).";
            return RedirectToAction(nameof(Index));
        }

        var created = 0;
        var skipped = 0;
        var errors = new List<string>();

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.FirstOrDefault();
        if (ws == null)
        {
            TempData["Error"] = "File Excel không có dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        var rows = ws.RowsUsed().Skip(1);
        foreach (var row in rows)
        {
            var ma = row.Cell(1).GetString().Trim();
            var hoTen = row.Cell(2).GetString().Trim();
            if (string.IsNullOrEmpty(ma) || string.IsNullOrEmpty(hoTen))
            {
                skipped++;
                continue;
            }

            if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == ma))
            {
                errors.Add($"{ma} - {hoTen}: mã sinh viên đã tồn tại");
                skipped++;
                continue;
            }

            var lopId = default(int?);
            var tenLop = row.Cell(8).GetString().Trim();
            if (!string.IsNullOrEmpty(tenLop))
            {
                var lop = await _db.Lops.FirstOrDefaultAsync(l => l.TenLop == tenLop);
                if (lop != null)
                    lopId = lop.Id;
            }

            var ngaySinh = default(DateTime?);
            var ngSinhStr = row.Cell(3).GetString().Trim();
            if (!string.IsNullOrEmpty(ngSinhStr) && DateTime.TryParseExact(ngSinhStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var ngSinh))
                ngaySinh = ngSinh;

            var sinhVien = new SinhVien
            {
                MaSinhVien = ma,
                HoTen = hoTen,
                NgaySinh = ngaySinh,
                GioiTinh = row.Cell(4).GetString().Trim() ?? "",
                Email = row.Cell(5).GetString().Trim() ?? "",
                SoDienThoai = row.Cell(6).GetString().Trim() ?? "",
                DiaChi = row.Cell(7).GetString().Trim() ?? "",
                LopId = lopId,
                NienKhoa = row.Cell(9).GetString().Trim() ?? "",
                TrangThai = string.IsNullOrEmpty(row.Cell(10).GetString().Trim()) ? "Đang học" : row.Cell(10).GetString().Trim(),
                HinhAnh = null
            };

            _db.SinhViens.Add(sinhVien);
            created++;
        }

        await _db.SaveChangesAsync();

        TempData["Msg"] = $"Đã nhập thành công {created} sinh viên từ Excel." + (skipped > 0 ? $" Bỏ qua {skipped} dòng." : "");
        if (errors.Any())
            TempData["Error"] = string.Join("; ", errors);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sinhVien = await _db.SinhViens
            .Include(s => s.Diems)
                .ThenInclude(d => d.MonHoc)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sinhVien == null)
        {
            return NotFound();
        }

        await LoadLopAsync(sinhVien.LopId);

        return View("Form", new SinhVienFormViewModel
        {
            Id = sinhVien.Id,
            MaSinhVien = sinhVien.MaSinhVien,
            HoTen = sinhVien.HoTen,
            NgaySinh = sinhVien.NgaySinh,
            GioiTinh = sinhVien.GioiTinh,
            Email = sinhVien.Email,
            SoDienThoai = sinhVien.SoDienThoai,
            DiaChi = sinhVien.DiaChi,
            LopId = sinhVien.LopId,
            NienKhoa = sinhVien.NienKhoa,
            TrangThai = sinhVien.TrangThai,
            Gpa = sinhVien.Gpa,
            XepLoai = sinhVien.XepLoai,
            HinhAnh = sinhVien.HinhAnh
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SinhVienFormViewModel model)
    {
        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien == null)
        {
            return NotFound();
        }

        Normalize(model);

        if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == model.MaSinhVien && s.Id != id))
        {
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadLopAsync(model.LopId);
            return View("Form", model);
        }

        sinhVien.MaSinhVien = model.MaSinhVien;
        sinhVien.HoTen = model.HoTen;
        sinhVien.NgaySinh = model.NgaySinh;
        sinhVien.GioiTinh = model.GioiTinh;
        sinhVien.Email = model.Email;
        sinhVien.SoDienThoai = model.SoDienThoai;
        sinhVien.DiaChi = model.DiaChi;
        sinhVien.LopId = model.LopId;
        sinhVien.NienKhoa = model.NienKhoa;
        sinhVien.TrangThai = model.TrangThai;

        if (model.FileAnh != null && model.FileAnh.Length > 0)
        {
            var avatarPath = await _fileUploadService.UploadAvatarAsync(model.FileAnh, sinhVien.HinhAnh);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                sinhVien.HinhAnh = avatarPath;
            }
        }

        // Đồng bộ sang ApplicationUser nếu tài khoản đã liên kết
        if (sinhVien.UserId != null)
        {
            var user = await _userManager.FindByIdAsync(sinhVien.UserId);
            if (user != null)
            {
                user.HoTen = sinhVien.HoTen;
                user.Email = sinhVien.Email;
                user.PhoneNumber = sinhVien.SoDienThoai;
                await _userManager.UpdateAsync(user);
            }
        }

        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã cập nhật sinh viên";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sinhVien = await _db.SinhViens
            .Include(s => s.DiemDanhs)
            .Include(s => s.Diems)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sinhVien == null)
        {
            return NotFound();
        }

        if (sinhVien.UserId == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Không thể xóa hồ sơ đang gắn với tài khoản Admin hiện tại.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        
        // Xóa các bản ghi phụ thuộc nếu có
        if (sinhVien.DiemDanhs.Any())
        {
            _db.DiemDanhs.RemoveRange(sinhVien.DiemDanhs);
        }
        if (sinhVien.Diems.Any())
        {
            _db.Diems.RemoveRange(sinhVien.Diems);
        }

        _db.SinhViens.Remove(sinhVien);
        _fileUploadService.DeleteFile(sinhVien.HinhAnh);
        await _db.SaveChangesAsync();

        if (sinhVien.UserId != null)
        {
            var user = await _userManager.FindByIdAsync(sinhVien.UserId);
            if (user != null)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Không thể xóa tài khoản sinh viên: {string.Join("; ", deleteResult.Errors.Select(error => error.Description))}";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        await transaction.CommitAsync();
        TempData["Msg"] = "Đã xóa sinh viên, điểm học phần và tài khoản liên kết (nếu có).";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(SinhVienFormViewModel model)
    {
        model.MaSinhVien = (model.MaSinhVien ?? "").Trim();
        model.HoTen = (model.HoTen ?? "").Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
        model.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();
        model.NienKhoa = string.IsNullOrWhiteSpace(model.NienKhoa) ? null : model.NienKhoa.Trim();
        model.TrangThai = string.IsNullOrWhiteSpace(model.TrangThai) ? "Đang học" : model.TrangThai.Trim();
    }

    private async Task LoadLopAsync(int? selected = null)
    {
        var lops = await _db.Lops
            .Include(lop => lop.Nganh)
            .ThenInclude(nganh => nganh!.DonVi)
            .OrderBy(lop => lop.TenLop)
            .ToListAsync();

        ViewBag.DanhSachLop = new SelectList(lops, "Id", "TenLop", selected);
        ViewBag.KhoaTheoLop = lops.ToDictionary(
            lop => lop.Id.ToString(),
            lop => lop.Nganh?.DonVi?.TenDonVi ?? lop.Nganh?.TenKhoa ?? "");
        ViewBag.KhoaHienTai = selected.HasValue
            ? lops.FirstOrDefault(lop => lop.Id == selected.Value)?.Nganh?.DonVi?.TenDonVi
                ?? lops.FirstOrDefault(lop => lop.Id == selected.Value)?.Nganh?.TenKhoa
                ?? ""
            : "";
    }
}
