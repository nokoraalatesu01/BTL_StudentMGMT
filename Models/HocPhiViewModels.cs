using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BTL_StudentMGMT.Models;

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

public class SinhVienHocPhiViewModel
{
    public IReadOnlyList<HocPhiHoaDonListItemViewModel> HoaDons { get; set; } = [];
    public decimal TongCongNo => HoaDons.Sum(item => item.ConNo);
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
