using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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
        else
        {
            TempData["Error"] = "Vui lòng chọn file hình ảnh hợp lệ để tải lên.";
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

    public async Task<IActionResult> HocPhi()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return View("Unlinked");

        var invoices = await _db.HoaDonHocPhis.AsNoTracking()
            .Where(invoice => invoice.SinhVienId == student.Id)
            .Include(invoice => invoice.HocKy).ThenInclude(semester => semester.NamHoc)
            .Include(invoice => invoice.ChiTietHoaDonHocPhis)
            .Include(invoice => invoice.ThanhToans.OrderByDescending(payment => payment.ThoiGianThanhToan))
            .OrderByDescending(invoice => invoice.NgayLap)
            .ToListAsync();

        return View(new SinhVienHocPhiViewModel
        {
            HoaDons = invoices.Select(invoice => new HocPhiHoaDonListItemViewModel
            {
                HoaDon = invoice,
                IdempotencyKey = Guid.NewGuid().ToString("N"),
                TongTien = GetInvoiceTotal(invoice),
                DaThanhToan = invoice.ThanhToans
                    .Where(payment => payment.TrangThai == "COMPLETED")
                    .Sum(payment => payment.SoTien)
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiYeuCauThanhToan(GuiYeuCauThanhToanViewModel model)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return View("Unlinked");
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng chọn ảnh biên lai chuyển khoản hợp lệ.";
            return RedirectToAction(nameof(HocPhi));
        }

        var existingRequest = await _db.ThanhToans.AsNoTracking()
            .Where(payment => payment.IdempotencyKey == model.IdempotencyKey)
            .Select(payment => new { payment.HoaDonId, payment.HoaDon.SinhVienId })
            .FirstOrDefaultAsync();
        if (existingRequest != null)
        {
            if (existingRequest.HoaDonId != model.HoaDonId || existingRequest.SinhVienId != student.Id)
            {
                return BadRequest();
            }

            TempData["Msg"] = "Yêu cầu này đã được gửi trước đó.";
            return RedirectToAction(nameof(HocPhi));
        }

        var invoice = await _db.HoaDonHocPhis
            .Where(item => item.HoaDonId == model.HoaDonId && item.SinhVienId == student.Id)
            .Include(item => item.ChiTietHoaDonHocPhis)
            .Include(item => item.ThanhToans)
            .FirstOrDefaultAsync();
        if (invoice == null) return NotFound();
        if (invoice.TrangThai is "CANCELLED" or "PAID")
        {
            TempData["Error"] = "Hóa đơn này không còn khoản cần thanh toán.";
            return RedirectToAction(nameof(HocPhi));
        }

        if (invoice.ThanhToans.Any(payment => payment.TrangThai == "PENDING"))
        {
            TempData["Error"] = "Hóa đơn đã có yêu cầu thanh toán đang chờ xác nhận.";
            return RedirectToAction(nameof(HocPhi));
        }

        var total = GetInvoiceTotal(invoice);
        var paid = invoice.ThanhToans.Where(payment => payment.TrangThai == "COMPLETED").Sum(payment => payment.SoTien);
        var amountDue = Math.Max(0, total - paid);
        if (amountDue <= 0)
        {
            TempData["Error"] = "Hóa đơn này không còn khoản cần thanh toán.";
            return RedirectToAction(nameof(HocPhi));
        }

        var proofFile = model.AnhMinhChung!;
        if (proofFile.Length == 0 || proofFile.Length > 5 * 1024 * 1024
            || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(proofFile.FileName).ToLowerInvariant()))
        {
            TempData["Error"] = "Ảnh biên lai phải có định dạng JPG, PNG hoặc WEBP và không quá 5MB.";
            return RedirectToAction(nameof(HocPhi));
        }

        var proofName = await _fileUploadService.UploadPaymentProofAsync(proofFile);
        if (proofName == null)
        {
            TempData["Error"] = "Không thể tải ảnh biên lai. Vui lòng kiểm tra định dạng và dung lượng tệp.";
            return RedirectToAction(nameof(HocPhi));
        }

        var transactionCode = await GenerateTransactionCodeAsync();
        _db.ThanhToans.Add(new ThanhToan
        {
            HoaDonId = invoice.HoaDonId,
            MaGiaoDich = transactionCode,
            SoTien = amountDue,
            ThoiGianThanhToan = DateTime.UtcNow,
            PhuongThuc = "CHUYEN_KHOAN",
            TrangThai = "PENDING",
            NguonThanhToan = "STUDENT_PORTAL",
            IdempotencyKey = model.IdempotencyKey,
            AnhMinhChung = proofName
        });
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            _fileUploadService.DeletePaymentProof(proofName);
            TempData["Error"] = "Một yêu cầu thanh toán cho hóa đơn này vừa được gửi. Vui lòng tải lại trang.";
            return RedirectToAction(nameof(HocPhi));
        }

        TempData["Msg"] = $"Đã gửi yêu cầu. Hãy ghi mã {transactionCode} trong nội dung chuyển khoản và chờ nhà trường xác nhận.";
        return RedirectToAction(nameof(HocPhi));
    }

    [HttpGet]
    public async Task<IActionResult> XemBienLai(long id)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return Forbid();
        var proofName = await _db.ThanhToans.AsNoTracking()
            .Where(payment => payment.ThanhToanId == id && payment.HoaDon.SinhVienId == student.Id)
            .Select(payment => payment.AnhMinhChung)
            .FirstOrDefaultAsync();
        if (proofName == null) return NotFound();

        var stream = await _fileUploadService.OpenPaymentProofAsync(proofName);
        if (stream == null) return NotFound();
        var contentType = Path.GetExtension(proofName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
        return File(stream, contentType);
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

    private async Task<SinhVien?> GetCurrentStudentAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return null;
        return await _db.SinhViens.FirstOrDefaultAsync(student => student.UserId == userId);
    }

    private static decimal GetInvoiceTotal(HoaDonHocPhi invoice) =>
        invoice.ChiTietHoaDonHocPhis.Sum(item => Math.Max(0, item.SoLuong * item.DonGia - item.SoTienGiam));

    private async Task<string> GenerateTransactionCodeAsync()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = new string(Enumerable.Range(0, 8)
                .Select(_ => letters[RandomNumberGenerator.GetInt32(letters.Length)])
                .ToArray());
            if (!await _db.ThanhToans.AnyAsync(payment => payment.MaGiaoDich == code))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Không thể tạo mã giao dịch duy nhất sau nhiều lần thử.");
    }
}
