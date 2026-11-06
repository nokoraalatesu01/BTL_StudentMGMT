using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BTL_StudentMGMT.Models;

public class SinhVienHocPhiViewModel
{
    public SinhVien SinhVien { get; set; } = null!;
    public decimal TongHocPhi { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ChuaThanhToan => Math.Max(0, TongHocPhi - DaThanhToan);
    public int SoKhoanConNo { get; set; }

    public List<HoaDonHocPhiItemViewModel> DanhSachHocPhi { get; set; } = new();
    public List<HocKy> DanhSachHocKy { get; set; } = new();
    public int? SelectedHocKyId { get; set; }
    public string? SelectedTrangThai { get; set; }
    public VietQrConfig VietQrConfig { get; set; } = new();
}

public class HoaDonHocPhiItemViewModel
{
    public long Id { get; set; }
    public string SoHoaDon { get; set; } = string.Empty;
    public string TenKhoanThu { get; set; } = string.Empty;
    public string TenHocKy { get; set; } = string.Empty;
    public string? TenNamHoc { get; set; }
    public decimal SoTien { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConPhaiDong => Math.Max(0, SoTien - DaThanhToan);
    public DateTime NgayLap { get; set; }
    public DateOnly? HanThanhToan { get; set; }
    public string TrangThai { get; set; } = "CHUA_THANH_TOAN";
    public string? GhiChu { get; set; }
    public bool CoTheThanhToan => ConPhaiDong > 0;
    public bool DangXuLy { get; set; } = false;
    public string? ActiveTransactionCode { get; set; }
}

public class TaoPaymentRequest
{
    public long HoaDonId { get; set; }
}

public class TaoPaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionCode { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
    public string BankId { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentContent { get; set; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
    public bool IsSandbox { get; set; }
}

public class CheckPaymentStatusResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = "PENDING";
    public string Message { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionCode { get; set; }
    public string? BankTransactionId { get; set; }
}

public class VietQrWebhookPayload
{
    public string? TransactionId { get; set; }
    public string? BankTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Content { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? Signature { get; set; }
    public string? Gateway { get; set; }
}

// ====================== VIEWMODELS DÀNH CHO ADMIN ======================

public class AdminPaymentDashboardViewModel
{
    public decimal TongHocPhiPhaiThu { get; set; }
    public decimal TongTienDaThu { get; set; }
    public decimal TongTienChuaThu => Math.Max(0, TongHocPhiPhaiThu - TongTienDaThu);
    public int SoGiaoDichThanhCong { get; set; }
    public int SoGiaoDichDangCho { get; set; }
    public int SoGiaoDichThatBai { get; set; }
    public int TongSoGiaoDich => SoGiaoDichThanhCong + SoGiaoDichDangCho + SoGiaoDichThatBai;

    // Bộ lọc
    public int? SelectedNamHocId { get; set; }
    public int? SelectedHocKyId { get; set; }
    public int? SelectedThang { get; set; }
    public string? SelectedTuNgay { get; set; }
    public string? SelectedDenNgay { get; set; }
    public int? SelectedLopId { get; set; }
    public int? SelectedNganhId { get; set; }

    public List<NamHoc> DanhSachNamHoc { get; set; } = new();
    public List<HocKy> DanhSachHocKy { get; set; } = new();
    public List<Lop> DanhSachLop { get; set; } = new();
    public List<Nganh> DanhSachNganh { get; set; } = new();

    // Dữ liệu biểu đồ
    public List<string> ChartThangLabels { get; set; } = new();
    public List<decimal> ChartThangData { get; set; } = new();

    public int ChartTyLePaid { get; set; }
    public int ChartTyLePending { get; set; }
    public int ChartTyLeFailed { get; set; }

    public List<string> ChartHocKyLabels { get; set; } = new();
    public List<decimal> ChartHocKyData { get; set; } = new();

    public List<string> ChartLopLabels { get; set; } = new();
    public List<double> ChartLopTyLeData { get; set; } = new();

    public List<Payment> RecentPayments { get; set; } = new();
    public List<PaymentWebhookLog> RecentLogs { get; set; } = new();
}

public class AdminKhoanThuListViewModel
{
    public List<DotThuHocPhi> DanhSachDotThu { get; set; } = new();
    public List<HocKy> DanhSachHocKy { get; set; } = new();
    public int? HocKyId { get; set; }
    public string? TrangThai { get; set; }
    public string? Search { get; set; }

    public int TongSoKhoanThu { get; set; }
    public decimal TongSoTienKeHoach { get; set; }
}

public class AdminTaoKhoanThuViewModel
{
    public int Id { get; set; }
    public string MaKhoanThu { get; set; } = string.Empty;
    public string TenKhoanThu { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public int HocKyId { get; set; }
    public decimal SoTien { get; set; }
    public DateTime NgayBatDau { get; set; } = DateTime.Today;
    public DateOnly? HanThanhToan { get; set; }
    public string DoiTuongApDung { get; set; } = "TOAN_TRUONG"; // TOAN_TRUONG, THEO_NGANH, THEO_LOP, THEO_SINH_VIEN
    public string? GiaTriApDung { get; set; }
    public bool PhatHanhHoaDon { get; set; } = true;

    public List<HocKy> DanhSachHocKy { get; set; } = new();
    public List<Nganh> DanhSachNganh { get; set; } = new();
    public List<Lop> DanhSachLop { get; set; } = new();
}

public class AdminChiTietKhoanThuViewModel
{
    public DotThuHocPhi DotThu { get; set; } = null!;
    public int TongSinhVien { get; set; }
    public int SinhVienDaDong { get; set; }
    public int SinhVienChuaDong { get; set; }
    public decimal TongPhaiThu { get; set; }
    public decimal TongDaThu { get; set; }
    public decimal TongConNo => Math.Max(0, TongPhaiThu - TongDaThu);
    public List<HoaDonHocPhi> DanhSachHoaDon { get; set; } = new();
    public bool CoTheSuaSoTien { get; set; } = true;
}

public class AdminGiaoDichListViewModel
{
    public List<Payment> DanhSachGiaoDich { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? BankId { get; set; }
    public string? TuNgay { get; set; }
    public string? DenNgay { get; set; }

    public decimal TongTienFilter { get; set; }
}

public class AdminChiTietGiaoDichViewModel
{
    public Payment Payment { get; set; } = null!;
    public BankTransaction? BankTransaction { get; set; }
    public List<PaymentWebhookLog> WebhookLogs { get; set; } = new();
    public List<NhatKyThanhToan> AuditLogs { get; set; } = new();
}

public class AdminCongNoListViewModel
{
    public List<SinhVienCongNoItem> DanhSachCongNo { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public decimal TongPhaiThu { get; set; }
    public decimal TongDaThu { get; set; }
    public decimal TongConNo { get; set; }
    public int SoSinhVienConNo { get; set; }

    public string? Search { get; set; }
    public int? LopId { get; set; }
    public int? NganhId { get; set; }
    public int? HocKyId { get; set; }
    public bool ChiSinhVienNo { get; set; }

    public List<Lop> DanhSachLop { get; set; } = new();
    public List<Nganh> DanhSachNganh { get; set; } = new();
    public List<HocKy> DanhSachHocKy { get; set; } = new();
}

public class SinhVienCongNoItem
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public string TenNganh { get; set; } = string.Empty;
    public decimal TongHocPhi { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConNo => Math.Max(0, TongHocPhi - DaThanhToan);
    public int SoKhoanChuaThanhToan { get; set; }
    public string TrangThaiCongNo => ConNo == 0 ? "HOAN_THANH" : "CON_NO";
}

public class AdminChiTietCongNoViewModel
{
    public SinhVien SinhVien { get; set; } = null!;
    public decimal TongHocPhi { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConNo => Math.Max(0, TongHocPhi - DaThanhToan);
    public List<HoaDonHocPhi> DanhSachHoaDon { get; set; } = new();
    public List<Payment> LichSuThanhToan { get; set; } = new();
    public List<NhatKyThanhToan> NhatKyDieuChinh { get; set; } = new();
}

public class AdminDoiSoatViewModel
{
    public List<DoiSoatItemViewModel> DanhSachDoiSoat { get; set; } = new();
    public int TongGiaoDichNganHang { get; set; }
    public decimal TongTienBaoCo { get; set; }
    public int SoGiaoDichKhop { get; set; }
    public int SoGiaoDichBatThuong { get; set; }

    public string? KetQuaFilter { get; set; }
    public string? TuNgay { get; set; }
    public string? DenNgay { get; set; }
    public string? Search { get; set; }
}

public class DoiSoatItemViewModel
{
    public long BankTransactionDbId { get; set; }
    public string BankTransactionId { get; set; } = string.Empty;
    public string TransactionContent { get; set; } = string.Empty;
    public decimal AmountBank { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? PaymentTransactionCode { get; set; }
    public decimal? AmountInternal { get; set; }
    public string? InternalStatus { get; set; }
    public string? StudentInfo { get; set; }
    public string KetQuaDoiSoat { get; set; } = "MATCHED"; // MATCHED, UNMATCHED_NO_ORDER, AMOUNT_MISMATCH, CONTENT_MISMATCH, DUPLICATE, MANUAL_REVIEW
    public string? GhiChu { get; set; }
    public bool DaXuLy { get; set; }
}

public class DieuChinhCongNoRequest
{
    public long HoaDonId { get; set; }
    public decimal SoTienMoi { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public string? BangChung { get; set; }
}

public class XuLyBatThuongRequest
{
    public long BankTransactionDbId { get; set; }
    public string? PaymentCode { get; set; }
    public string HanhDong { get; set; } = string.Empty; // XAC_NHAN_KHOP, HUY_BO, DANH_DAU_CHO_XU_LY
    public string LyDo { get; set; } = string.Empty;
    public string? BangChung { get; set; }
}


// ====================== VIEWMODELS DÀNH CHO QUẢN LÝ HỌC PHÍ ======================

public class HocPhiHoaDonFormViewModel
{
    public long? HoaDonId { get; set; }

    [Display(Name = "Sinh viên")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sinh viên.")]
    public int SinhVienId { get; set; }

    [Display(Name = "Học kỳ")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn học kỳ.")]
    public int HocKyId { get; set; }

    [Display(Name = "Biểu phí")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn biểu phí.")]
    public int BieuPhiId { get; set; }

    [Display(Name = "Số tín chỉ")]
    [Range(0.01, 1000, ErrorMessage = "Số tín chỉ phải lớn hơn 0.")]
    public decimal SoTinChi { get; set; }

    [Display(Name = "Hạn thanh toán")]
    public DateOnly? HanThanhToan { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(500)]
    public string? GhiChu { get; set; }
}

public class BieuPhiHocPhiFormViewModel
{
    public int? BieuPhiId { get; set; }

    [Display(Name = "Chương trình đào tạo")]
    public int? ChuongTrinhId { get; set; }

    [Display(Name = "Năm học")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn năm học.")]
    public int NamHocId { get; set; }

    [Display(Name = "Đơn giá/tín chỉ")]
    [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "Đơn giá phải lớn hơn 0.")]
    public decimal DonGiaTinChi { get; set; }

    [Display(Name = "Hiệu lực từ")]
    public DateOnly HieuLucTu { get; set; }

    [Display(Name = "Hiệu lực đến")]
    public DateOnly? HieuLucDen { get; set; }
}

public class HocPhiHoaDonListItemViewModel
{
    public HoaDonHocPhi HoaDon { get; set; } = null!;
    public decimal TongTien { get; set; }
    public decimal DaThanhToan { get; set; }
    public string IdempotencyKey { get; set; } = "";
    public decimal ConNo => HoaDon.TrangThai == "CANCELLED" ? 0 : Math.Max(0, TongTien - DaThanhToan);
}

public class HocPhiChiTietViewModel
{
    public HoaDonHocPhi HoaDon { get; set; } = null!;
    public decimal TongTien { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConNo => HoaDon.TrangThai == "CANCELLED" ? 0 : Math.Max(0, TongTien - DaThanhToan);
}

public class BieuPhiHocPhiListItemViewModel
{
    public int BieuPhiId { get; set; }
    public string NamHoc { get; set; } = "";
    public string TenChuongTrinh { get; set; } = "";
    public decimal DonGiaTinChi { get; set; }
    public DateOnly HieuLucTu { get; set; }
    public DateOnly? HieuLucDen { get; set; }
}

public class GuiYeuCauThanhToanViewModel
{
    public long HoaDonId { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 32)]
    [RegularExpression("^[a-fA-F0-9]{32}$")]
    public string IdempotencyKey { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng tải ảnh biên lai chuyển khoản.")]
    [Display(Name = "Ảnh biên lai")]
    public IFormFile? AnhMinhChung { get; set; }
}
