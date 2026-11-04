using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class NhatKyThanhToan
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Hành động")]
    public string HanhDong { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DoiTuong { get; set; } = string.Empty;

    [StringLength(100)]
    public string? KhoaChinh { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Người thực hiện")]
    public string NguoiThucHien { get; set; } = "Admin";

    public DateTime ThoiGian { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(500)]
    [Display(Name = "Lý do")]
    public string LyDo { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Bằng chứng đối chiếu")]
    public string? BangChungDoiChieu { get; set; }

    [StringLength(100)]
    public string? TrangThaiTruoc { get; set; }

    [StringLength(100)]
    public string? TrangThaiSau { get; set; }

    public string? ChiTiet { get; set; }
}
