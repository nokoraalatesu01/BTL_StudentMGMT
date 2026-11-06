using System.Data;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class QuanLyHocPhiController : Controller
{
    private const string Pending = "PENDING";
    private const string Completed = "COMPLETED";
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _fileUploadService;

    public QuanLyHocPhiController(ApplicationDbContext db, IFileUploadService fileUploadService)
    {
        _db = db;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? hocKyId, string? trangThai)
    {
        var query = _db.HoaDonHocPhis.AsNoTracking()
            .Include(invoice => invoice.SinhVien)
            .Include(invoice => invoice.HocKy)
            .Include(invoice => invoice.ChiTietHoaDonHocPhis)
            .Include(invoice => invoice.ThanhToans)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(invoice => invoice.SoHoaDon.Contains(keyword)
                || invoice.SinhVien.MaSinhVien.Contains(keyword)
                || invoice.SinhVien.HoTen.Contains(keyword));
        }

        if (hocKyId.HasValue)
        {
            query = query.Where(invoice => invoice.HocKyId == hocKyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(invoice => invoice.TrangThai == trangThai);
        }

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.HocKyId = new SelectList(
            await _db.HocKies.AsNoTracking().Include(semester => semester.NamHoc)
                .OrderByDescending(semester => semester.NgayBatDau).ToListAsync(),
            nameof(HocKy.HocKyId),
            nameof(HocKy.TenHocKy),
            hocKyId);
        ViewBag.TrangThai = trangThai;

        var invoices = await query.OrderByDescending(invoice => invoice.NgayLap).ToListAsync();
        return View(invoices.Select(ToListItem).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> TaoHoaDon()
    {
        await LoadInvoiceFormOptionsAsync();
        return View("Form", new HocPhiHoaDonFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoHoaDon(HocPhiHoaDonFormViewModel model)
    {
        var fee = await FindApplicableFeeAsync(model.SinhVienId, model.HocKyId, model.BieuPhiId);
        if (fee == null)
        {
            ModelState.AddModelError(nameof(model.BieuPhiId), "Biểu phí không áp dụng cho sinh viên hoặc học kỳ đã chọn.");
        }
        else if (model.SoTinChi * fee.DonGiaTinChi > 9999999999999999.99m)
        {
            ModelState.AddModelError(nameof(model.SoTinChi), "Tổng học phí vượt giới hạn lưu trữ.");
        }

        if (await _db.HoaDonHocPhis.AnyAsync(invoice => invoice.SinhVienId == model.SinhVienId && invoice.HocKyId == model.HocKyId))
        {
            ModelState.AddModelError(nameof(model.HocKyId), "Sinh viên đã có hóa đơn trong học kỳ này.");
        }

        if (!ModelState.IsValid || fee == null)
        {
            await LoadInvoiceFormOptionsAsync(model);
            return View("Form", model);
        }

        var invoice = new HoaDonHocPhi
        {
            SoHoaDon = $"HP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..34].ToUpperInvariant(),
            SinhVienId = model.SinhVienId,
            HocKyId = model.HocKyId,
            NgayLap = DateTime.UtcNow,
            HanThanhToan = model.HanThanhToan,
            TrangThai = "ISSUED",
            GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim(),
            ChiTietHoaDonHocPhis =
            [
                new ChiTietHoaDonHocPhi
                {
                    LoaiKhoanThu = "HOCPHI",
                    MoTa = $"Học phí {model.SoTinChi:0.##} tín chỉ",
                    SoLuong = model.SoTinChi,
                    DonGia = fee.DonGiaTinChi,
                    SoTienGiam = 0
                }
            ]
        };

        _db.HoaDonHocPhis.Add(invoice);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã tạo hóa đơn học phí.";
        return RedirectToAction(nameof(ChiTiet), new { id = invoice.Id });
    }

    [HttpGet]
    public async Task<IActionResult> SuaHoaDon(long id)
    {
        var invoice = await _db.HoaDonHocPhis
            .Include(item => item.ChiTietHoaDonHocPhis)
            .Include(item => item.ThanhToans)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (invoice == null) return NotFound();
        if (invoice.TrangThai == "CANCELLED" || invoice.ThanhToans.Any(payment => payment.TrangThai is Pending or Completed))
        {
            TempData["Error"] = "Không thể sửa hóa đơn đã hủy hoặc đã có giao dịch đang chờ/xác nhận.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        var detail = invoice.ChiTietHoaDonHocPhis.FirstOrDefault();
        var studentProgramId = await _db.SinhViens.AsNoTracking()
            .Where(item => item.Id == invoice.SinhVienId)
            .Select(item => item.Lop!.ChuongTrinhDaoTaoId)
            .FirstOrDefaultAsync();
        var semesterYearId = await _db.HocKies.AsNoTracking()
            .Where(item => item.HocKyId == invoice.HocKyId)
            .Select(item => item.NamHocId)
            .FirstAsync();
        var currentFeeId = detail == null
            ? null
            : await _db.BieuPhiHocPhis.AsNoTracking()
                .Where(fee => fee.NamHocId == semesterYearId && fee.DonGiaTinChi == detail.DonGia
                    && (fee.ChuongTrinhId == null || fee.ChuongTrinhId == studentProgramId))
                .Select(fee => (int?)fee.BieuPhiId)
                .FirstOrDefaultAsync();
        var model = new HocPhiHoaDonFormViewModel
        {
            HoaDonId = invoice.HoaDonId,
            SinhVienId = invoice.SinhVienId,
            HocKyId = invoice.HocKyId,
            BieuPhiId = currentFeeId ?? 0,
            SoTinChi = detail?.SoLuong ?? 0,
            HanThanhToan = invoice.HanThanhToan,
            GhiChu = invoice.GhiChu
        };
        await LoadInvoiceFormOptionsAsync(model, isEdit: true);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaHoaDon(long id, HocPhiHoaDonFormViewModel model)
    {
        if (model.HoaDonId != id) return BadRequest();
        var invoice = await _db.HoaDonHocPhis
            .Include(item => item.ChiTietHoaDonHocPhis)
            .Include(item => item.ThanhToans)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (invoice == null) return NotFound();
        if (invoice.TrangThai == "CANCELLED" || invoice.ThanhToans.Any(payment => payment.TrangThai is Pending or Completed))
        {
            TempData["Error"] = "Không thể sửa hóa đơn đã hủy hoặc đã có giao dịch đang chờ/xác nhận.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        var fee = await FindApplicableFeeAsync(model.SinhVienId, model.HocKyId, model.BieuPhiId);
        if (fee == null)
        {
            ModelState.AddModelError(nameof(model.BieuPhiId), "Biểu phí không áp dụng cho sinh viên hoặc học kỳ đã chọn.");
        }
        else if (model.SoTinChi * fee.DonGiaTinChi > 9999999999999999.99m)
        {
            ModelState.AddModelError(nameof(model.SoTinChi), "Tổng học phí vượt giới hạn lưu trữ.");
        }

        if (await _db.HoaDonHocPhis.AnyAsync(item => item.Id != id && item.SinhVienId == model.SinhVienId && item.HocKyId == model.HocKyId))
        {
            ModelState.AddModelError(nameof(model.HocKyId), "Sinh viên đã có hóa đơn trong học kỳ này.");
        }

        if (!ModelState.IsValid || fee == null)
        {
            await LoadInvoiceFormOptionsAsync(model, isEdit: true);
            return View("Form", model);
        }

        invoice.SinhVienId = model.SinhVienId;
        invoice.HocKyId = model.HocKyId;
        invoice.HanThanhToan = model.HanThanhToan;
        invoice.GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim();
        var detail = invoice.ChiTietHoaDonHocPhis.FirstOrDefault();
        if (detail == null)
        {
            detail = new ChiTietHoaDonHocPhi { HoaDonId = id };
            invoice.ChiTietHoaDonHocPhis.Add(detail);
        }

        detail.LoaiKhoanThu = "HOCPHI";
        detail.MoTa = $"Học phí {model.SoTinChi:0.##} tín chỉ";
        detail.SoLuong = model.SoTinChi;
        detail.DonGia = fee.DonGiaTinChi;
        detail.SoTienGiam = 0;
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã cập nhật hóa đơn.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ChiTiet(long id)
    {
        var invoice = await _db.HoaDonHocPhis.AsNoTracking()
            .Include(item => item.SinhVien)
            .Include(item => item.HocKy).ThenInclude(semester => semester.NamHoc)
            .Include(item => item.ChiTietHoaDonHocPhis)
            .Include(item => item.ThanhToans.OrderByDescending(payment => payment.ThoiGianThanhToan))
            .FirstOrDefaultAsync(item => item.Id == id);
        if (invoice == null) return NotFound();
        return View(new HocPhiChiTietViewModel
        {
            HoaDon = invoice,
            TongTien = GetInvoiceTotal(invoice),
            DaThanhToan = GetCompletedPayments(invoice)
        });
    }

    [HttpGet]
    public async Task<IActionResult> BieuPhi()
    {
        ViewBag.BieuPhis = await LoadFeeListAsync();
        await LoadFeeFormOptionsAsync();
        return View(new BieuPhiHocPhiFormViewModel { HieuLucTu = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BieuPhi(BieuPhiHocPhiFormViewModel model)
    {
        if (model.HieuLucDen < model.HieuLucTu)
        {
            ModelState.AddModelError(nameof(model.HieuLucDen), "Ngày hết hiệu lực phải sau ngày bắt đầu.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFeePageAsync(model);
            return View(model);
        }

        _db.BieuPhiHocPhis.Add(new BieuPhiHocPhi
        {
            ChuongTrinhId = model.ChuongTrinhId,
            NamHocId = model.NamHocId,
            DonGiaTinChi = model.DonGiaTinChi,
            LoaiTien = "VND",
            HieuLucTu = model.HieuLucTu,
            HieuLucDen = model.HieuLucDen
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã thêm biểu phí.";
        return RedirectToAction(nameof(BieuPhi));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyHoaDon(long id)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var invoice = await _db.HoaDonHocPhis.Include(item => item.ThanhToans)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (invoice == null) return NotFound();
        if (invoice.TrangThai == "PAID" || invoice.ThanhToans.Any(payment => payment.TrangThai == Completed))
        {
            TempData["Error"] = "Không thể hủy hóa đơn đã có khoản thanh toán được xác nhận.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        foreach (var payment in invoice.ThanhToans.Where(payment => payment.TrangThai == Pending))
        {
            payment.TrangThai = "REJECTED";
            payment.GhiChu = "Hóa đơn đã bị hủy.";
        }

        invoice.TrangThai = "CANCELLED";
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Msg"] = "Đã hủy hóa đơn.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanThanhToan(long id, bool chapNhan, string? ghiChu)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var payment = await _db.ThanhToans.Include(item => item.HoaDon)
            .ThenInclude(invoice => invoice.ChiTietHoaDonHocPhis)
            .FirstOrDefaultAsync(item => item.ThanhToanId == id);
        if (payment == null) return NotFound();
        if (payment.TrangThai != Pending || payment.HoaDon.TrangThai == "CANCELLED")
        {
            TempData["Error"] = "Giao dịch không còn ở trạng thái chờ xác nhận.";
            return RedirectToAction(nameof(ChiTiet), new { id = payment.HoaDonId });
        }

        payment.TrangThai = chapNhan ? Completed : "REJECTED";
        payment.GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();
        if (chapNhan)
        {
            var paidBefore = await _db.ThanhToans
                .Where(item => item.HoaDonId == payment.HoaDonId && item.TrangThai == Completed && item.ThanhToanId != payment.ThanhToanId)
                .SumAsync(item => (decimal?)item.SoTien) ?? 0;
            var total = GetInvoiceTotal(payment.HoaDon);
            if (payment.SoTien <= 0 || payment.SoTien > total - paidBefore)
            {
                TempData["Error"] = "Số tiền giao dịch vượt quá công nợ còn lại; chưa thể xác nhận.";
                return RedirectToAction(nameof(ChiTiet), new { id = payment.HoaDonId });
            }

            var paidTotal = paidBefore + payment.SoTien;
            payment.HoaDon.TrangThai = paidTotal >= total ? "PAID" : "PARTIAL";
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Msg"] = chapNhan ? "Đã xác nhận giao dịch thanh toán." : "Đã từ chối giao dịch.";
        return RedirectToAction(nameof(ChiTiet), new { id = payment.HoaDonId });
    }

    [HttpGet]
    public async Task<IActionResult> XemMinhChung(long id)
    {
        var fileName = await _db.ThanhToans.AsNoTracking()
            .Where(payment => payment.ThanhToanId == id)
            .Select(payment => payment.AnhMinhChung)
            .FirstOrDefaultAsync();
        if (fileName == null) return NotFound();
        return await ServeProofAsync(fileName);
    }

    private async Task<IActionResult> ServeProofAsync(string fileName)
    {
        var stream = await _fileUploadService.OpenPaymentProofAsync(fileName);
        if (stream == null) return NotFound();
        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
        return File(stream, contentType);
    }

    private async Task<BieuPhiHocPhi?> FindApplicableFeeAsync(int sinhVienId, int hocKyId, int bieuPhiId)
    {
        var student = await _db.SinhViens.AsNoTracking()
            .Where(item => item.Id == sinhVienId)
            .Select(item => new { ProgramId = item.Lop!.ChuongTrinhDaoTaoId })
            .FirstOrDefaultAsync();
        var semester = await _db.HocKies.AsNoTracking()
            .Where(item => item.HocKyId == hocKyId)
            .Select(item => new { item.NamHocId, item.NgayBatDau })
            .FirstOrDefaultAsync();
        if (student == null || semester == null) return null;

        return await _db.BieuPhiHocPhis.AsNoTracking()
            .Where(fee => fee.BieuPhiId == bieuPhiId && fee.NamHocId == semester.NamHocId
                && (fee.ChuongTrinhId == null || fee.ChuongTrinhId == student.ProgramId)
                && fee.HieuLucTu <= semester.NgayBatDau
                && (fee.HieuLucDen == null || fee.HieuLucDen >= semester.NgayBatDau))
            .FirstOrDefaultAsync();
    }

    private async Task LoadInvoiceFormOptionsAsync(HocPhiHoaDonFormViewModel? model = null, bool isEdit = false)
    {
        ViewBag.SinhViens = new SelectList(
            await _db.SinhViens.AsNoTracking().OrderBy(item => item.MaSinhVien).ToListAsync(),
            nameof(SinhVien.Id), nameof(SinhVien.MaSinhVien), model?.SinhVienId);
        ViewBag.HocKies = new SelectList(
            await _db.HocKies.AsNoTracking().Include(item => item.NamHoc)
                .OrderByDescending(item => item.NgayBatDau).ToListAsync(),
            nameof(HocKy.HocKyId), nameof(HocKy.TenHocKy), model?.HocKyId);
        var fees = await (
            from fee in _db.BieuPhiHocPhis.AsNoTracking()
            join year in _db.NamHocs.AsNoTracking() on fee.NamHocId equals year.NamHocId
            join programOption in _db.ChuongTrinhDaoTaos.AsNoTracking() on fee.ChuongTrinhId equals programOption.Id into programs
            from program in programs.DefaultIfEmpty()
            select new
            {
                fee.BieuPhiId,
                Ten = year.MaNamHoc + " - " + (program == null ? "Tất cả chương trình" : program.TenChuongTrinh)
                    + " - " + fee.DonGiaTinChi + " VND/tín chỉ"
            }).ToListAsync();
        ViewBag.BieuPhis = new SelectList(
            fees,
            nameof(BieuPhiHocPhi.BieuPhiId), "Ten", model?.BieuPhiId);
        ViewBag.IsEdit = isEdit;
    }

    private async Task LoadFeeFormOptionsAsync(BieuPhiHocPhiFormViewModel? model = null)
    {
        ViewBag.NamHocs = new SelectList(await _db.NamHocs.AsNoTracking().OrderByDescending(item => item.NgayBatDau).ToListAsync(),
            nameof(NamHoc.NamHocId), nameof(NamHoc.MaNamHoc), model?.NamHocId);
        ViewBag.ChuongTrinhs = new SelectList(await _db.ChuongTrinhDaoTaos.AsNoTracking().OrderBy(item => item.TenChuongTrinh).ToListAsync(),
            nameof(ChuongTrinhDaoTao.Id), nameof(ChuongTrinhDaoTao.TenChuongTrinh), model?.ChuongTrinhId);
    }

    private async Task LoadFeePageAsync(BieuPhiHocPhiFormViewModel model)
    {
        await LoadFeeFormOptionsAsync(model);
        ViewBag.BieuPhis = await LoadFeeListAsync();
    }

    private Task<List<BieuPhiHocPhiListItemViewModel>> LoadFeeListAsync() =>
        _db.BieuPhiHocPhis.AsNoTracking()
            .Join(_db.NamHocs.AsNoTracking(), fee => fee.NamHocId, year => year.NamHocId,
                (fee, year) => new { fee, year.MaNamHoc })
            .GroupJoin(_db.ChuongTrinhDaoTaos.AsNoTracking(), item => item.fee.ChuongTrinhId,
                program => (int?)program.Id, (item, programs) => new { item, programs })
            .SelectMany(item => item.programs.DefaultIfEmpty(), (item, program) => new BieuPhiHocPhiListItemViewModel
            {
                BieuPhiId = item.item.fee.BieuPhiId,
                NamHoc = item.item.MaNamHoc,
                TenChuongTrinh = program == null ? "Tất cả chương trình" : program.TenChuongTrinh,
                DonGiaTinChi = item.item.fee.DonGiaTinChi,
                HieuLucTu = item.item.fee.HieuLucTu,
                HieuLucDen = item.item.fee.HieuLucDen
            })
            .OrderByDescending(item => item.HieuLucTu)
            .ToListAsync();

    private static HocPhiHoaDonListItemViewModel ToListItem(HoaDonHocPhi invoice) => new()
    {
        HoaDon = invoice,
        TongTien = GetInvoiceTotal(invoice),
        DaThanhToan = GetCompletedPayments(invoice)
    };

    private static decimal GetInvoiceTotal(HoaDonHocPhi invoice) =>
        invoice.ChiTietHoaDonHocPhis.Sum(item => Math.Max(0, item.SoLuong * item.DonGia - item.SoTienGiam));

    private static decimal GetCompletedPayments(HoaDonHocPhi invoice) =>
        invoice.ThanhToans.Where(payment => payment.TrangThai == Completed).Sum(payment => payment.SoTien);
}
