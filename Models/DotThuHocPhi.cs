using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class DotThuHocPhi
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Mã khoản thu")]
    public string MaKhoanThu { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Tên khoản thu")]
    public string TenKhoanThu { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required]
    [Display(Name = "Học kỳ")]
    public int HocKyId { get; set; }
    public virtual HocKy HocKy { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Số tiền")]
    public decimal SoTien { get; set; }

    [Display(Name = "Ngày bắt đầu")]
    public DateTime NgayBatDau { get; set; } = DateTime.UtcNow;

    [Display(Name = "Hạn thanh toán")]
    public DateOnly? HanThanhToan { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Đối tượng áp dụng")]
    public string DoiTuongApDung { get; set; } = "TOAN_TRUONG"; // TOAN_TRUONG, THEO_NGANH, THEO_LOP, THEO_SINH_VIEN

    [StringLength(255)]
    [Display(Name = "Giá trị đối tượng")]
    public string? GiaTriApDung { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "ACTIVE"; // ACTIVE, STOPPED, DRAFT

    public string? NguoiTao { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public virtual ICollection<HoaDonHocPhi> HoaDonHocPhis { get; set; } = new List<HoaDonHocPhi>();
}
