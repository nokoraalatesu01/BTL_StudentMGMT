using System;
using System.Text;
using System.Threading.Tasks;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class QuanLyThanhToanController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IVietQrService _vietQrService;

    public QuanLyThanhToanController(
        IPaymentService paymentService,
        IVietQrService vietQrService)
    {
        _paymentService = paymentService;
        _vietQrService = vietQrService;
    }

    // 1. Dashboard Tổng quan thanh toán
    public async Task<IActionResult> Index(
        int? namHocId, int? hocKyId, int? thang, string? tuNgay, string? denNgay, int? lopId, int? nganhId)
    {
        var model = await _paymentService.GetAdminDashboardAsync(namHocId, hocKyId, thang, tuNgay, denNgay, lopId, nganhId);
        return View(model);
    }

    // 2. Quản lý khoản thu học phí
    public async Task<IActionResult> KhoanThu(int? hocKyId, string? trangThai, string? search)
    {
        var model = await _paymentService.GetKhoanThuListAsync(hocKyId, trangThai, search);
        return View(model);
    }

    public async Task<IActionResult> ThemKhoanThu()
    {
        var model = await _paymentService.GetTaoKhoanThuModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemKhoanThu(AdminTaoKhoanThuViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var reloadModel = await _paymentService.GetTaoKhoanThuModelAsync();
            model.DanhSachHocKy = reloadModel.DanhSachHocKy;
            model.DanhSachNganh = reloadModel.DanhSachNganh;
            model.DanhSachLop = reloadModel.DanhSachLop;
            return View(model);
        }

        var userName = User.Identity?.Name ?? "Admin";
        var result = await _paymentService.TaoKhoanThuAsync(model, userName);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            var reloadModel = await _paymentService.GetTaoKhoanThuModelAsync();
            model.DanhSachHocKy = reloadModel.DanhSachHocKy;
            model.DanhSachNganh = reloadModel.DanhSachNganh;
            model.DanhSachLop = reloadModel.DanhSachLop;
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(ChiTietKhoanThu), new { id = result.Id });
    }

    public async Task<IActionResult> ChiTietKhoanThu(int id)
    {
        var model = await _paymentService.GetChiTietKhoanThuAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NgungKhoanThu(int id)
    {
        var userName = User.Identity?.Name ?? "Admin";
        var result = await _paymentService.NgungKhoanThuAsync(id, userName);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
        return RedirectToAction(nameof(ChiTietKhoanThu), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatKhoanThu(int id, string tenKhoanThu, string? moTa, DateOnly? hanThanhToan, decimal? soTienMoi)
    {
        var userName = User.Identity?.Name ?? "Admin";
        var result = await _paymentService.CapNhatKhoanThuAsync(id, tenKhoanThu, moTa, hanThanhToan, soTienMoi, userName);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
        return RedirectToAction(nameof(ChiTietKhoanThu), new { id });
    }

    // 3. Danh sách giao dịch
    public async Task<IActionResult> GiaoDich(
        int page = 1, int pageSize = 15, string? search = null, string? status = null, string? bankId = null, string? tuNgay = null, string? denNgay = null)
    {
        var model = await _paymentService.GetGiaoDichListAsync(page, pageSize, search, status, bankId, tuNgay, denNgay);
        return View(model);
    }

    public async Task<IActionResult> ChiTietGiaoDich(long id)
    {
        var model = await _paymentService.GetChiTietGiaoDichAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    public async Task<IActionResult> XuatExcelGiaoDich(
        string? search = null, string? status = null, string? bankId = null, string? tuNgay = null, string? denNgay = null)
    {
        var model = await _paymentService.GetGiaoDichListAsync(1, 10000, search, status, bankId, tuNgay, denNgay);
        
        var sb = new StringBuilder();
        sb.AppendLine("STT,Mã Giao Dịch,Mã Sinh Viên,Họ Tên,Lớp,Khoản Thu,Số Tiền (VNĐ),Phương Thức,Ngân Hàng,Thời Gian Tạo,Thời Gian Nhận Tiền,Trạng Thái,Mã GD Ngân Hàng");

        int stt = 1;
        foreach (var p in model.DanhSachGiaoDich)
        {
            var tenLop = p.SinhVien?.Lop?.TenLop ?? "";
            var tenKhoanThu = p.HoaDonHocPhi?.TenKhoanThu ?? "";
            var paidAtStr = p.PaidAt.HasValue ? p.PaidAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
            var createdAtStr = p.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");

            sb.AppendLine($"{stt++},\"{p.TransactionCode}\",\"{p.SinhVien?.MaSinhVien}\",\"{p.SinhVien?.HoTen}\",\"{tenLop}\",\"{tenKhoanThu}\",{p.Amount},\"{p.PaymentMethod}\",\"Vietcombank\",\"{createdAtStr}\",\"{paidAtStr}\",\"{p.Status}\",\"{p.BankTransactionId}\"");
        }

        var userName = User.Identity?.Name ?? "Admin";
        await _paymentService.GhiNhatKyAuditAsync("XUAT_BAO_CAO", "Payment", null, userName, $"Xuất báo cáo {model.DanhSachGiaoDich.Count} giao dịch");

        byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(buffer, "text/csv; charset=utf-8", $"GiaoDich_HocPhi_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    // 4. Quản lý công nợ sinh viên
    public async Task<IActionResult> CongNo(
        int page = 1, int pageSize = 15, string? search = null, int? lopId = null, int? nganhId = null, int? hocKyId = null, bool chiSinhVienNo = false)
    {
        var model = await _paymentService.GetCongNoListAsync(page, pageSize, search, lopId, nganhId, hocKyId, chiSinhVienNo);
        return View(model);
    }

    public async Task<IActionResult> ChiTietCongNo(int sinhVienId)
    {
        var model = await _paymentService.GetChiTietCongNoAsync(sinhVienId);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DieuChinhCongNo(DieuChinhCongNoRequest request, int sinhVienId)
    {
        var userName = User.Identity?.Name ?? "Admin";
        var result = await _paymentService.DieuChinhCongNoAsync(request, userName);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
        return RedirectToAction(nameof(ChiTietCongNo), new { sinhVienId });
    }

    public async Task<IActionResult> XuatExcelCongNo(
        string? search = null, int? lopId = null, int? nganhId = null, int? hocKyId = null, bool chiSinhVienNo = false)
    {
        var model = await _paymentService.GetCongNoListAsync(1, 10000, search, lopId, nganhId, hocKyId, chiSinhVienNo);

        var sb = new StringBuilder();
        sb.AppendLine("STT,Mã Sinh Viên,Họ Tên,Lớp,Ngành,Tổng Học Phí (VNĐ),Đã Thanh Toán (VNĐ),Còn Nợ (VNĐ),Số Khoản Nợ,Trạng Thái");

        int stt = 1;
        foreach (var c in model.DanhSachCongNo)
        {
            var trangThaiStr = c.ConNo == 0 ? "Đã hoàn thành" : "Còn nợ học phí";
            sb.AppendLine($"{stt++},\"{c.MaSinhVien}\",\"{c.HoTen}\",\"{c.TenLop}\",\"{c.TenNganh}\",{c.TongHocPhi},{c.DaThanhToan},{c.ConNo},{c.SoKhoanChuaThanhToan},\"{trangThaiStr}\"");
        }

        var userName = User.Identity?.Name ?? "Admin";
        await _paymentService.GhiNhatKyAuditAsync("XUAT_BAO_CAO", "HoaDonHocPhi", null, userName, $"Xuất báo cáo công nợ {model.DanhSachCongNo.Count} sinh viên");

        byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(buffer, "text/csv; charset=utf-8", $"CongNo_SinhVien_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    // 5. Đối soát ngân hàng Vietcombank
    public async Task<IActionResult> DoiSoat(string? ketQuaFilter, string? tuNgay, string? denNgay, string? search)
    {
        var model = await _paymentService.GetDoiSoatListAsync(ketQuaFilter, tuNgay, denNgay, search);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XuLyBatThuongDoiSoat(XuLyBatThuongRequest request)
    {
        var userName = User.Identity?.Name ?? "Admin";
        var result = await _paymentService.XuLyBatThuongDoiSoatAsync(request, userName);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
        return RedirectToAction(nameof(DoiSoat));
    }

    // 6. Cấu hình Vietcombank & VietQR
    public async Task<IActionResult> CauHinh()
    {
        var config = await _vietQrService.GetConfigAsync();
        return View(config);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CauHinh(VietQrConfig model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _vietQrService.UpdateConfigAsync(model);
        var userName = User.Identity?.Name ?? "Admin";
        await _paymentService.GhiNhatKyAuditAsync("CAP_NHAT_CAU_HINH", "VietQrConfig", model.Id.ToString(), userName, "Cập nhật cấu hình tài khoản Vietcombank & VietQR");

        TempData["Success"] = "Đã cập nhật cấu hình tài khoản Vietcombank & VietQR thành công!";
        return RedirectToAction(nameof(CauHinh));
    }
}
